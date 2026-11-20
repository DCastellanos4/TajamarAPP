using System.Data;
using Microsoft.Data.SqlClient;

static class Reservations
{
    private sealed record Trip(int DriverId, int Available, bool Active, DateTimeOffset Start);

    public static void MapReservations(this RouteGroupBuilder api)
    {
        api.MapPost("/trayectos/{id:int}/reservar", (int id, HttpContext http, Database db, TimeProvider clock,
            CancellationToken ct) => Join(id, TeamJwt.UserId(http.User), false, db, clock, ct));
        api.MapPost("/trayectos/{id:int}/cola", (int id, HttpContext http, Database db, TimeProvider clock,
            CancellationToken ct) => Join(id, TeamJwt.UserId(http.User), true, db, clock, ct));
        api.MapDelete("/reservas/{id:long}", (long id, HttpContext http, Database db, TimeProvider clock,
            CancellationToken ct) => Cancel(id, TeamJwt.UserId(http.User), db, clock, ct));
    }

    private static Task<IResult> Join(int id, int userId, bool wait, Database db, TimeProvider clock,
        CancellationToken ct) => db.Write<IResult>(async (connection, transaction) =>
    {
        var trip = await LockTrip(connection, transaction, id, ct);
        if (trip is null)
            return ApiError.Result(404, "TRAYECTO_NO_ENCONTRADO", "Trayecto no encontrado.");
        if (trip.DriverId == userId)
            return ApiError.Result(409, "TRAYECTO_PROPIO", "El conductor no puede ocupar una plaza de su trayecto.");
        if (!trip.Active || trip.Start <= clock.GetUtcNow())
            return ApiError.Result(409, "TRAYECTO_CERRADO", "El trayecto ya no admite reservas.");

        using var existing = Database.Command(connection, transaction, """
            SELECT CASE WHEN EXISTS (SELECT 1 FROM movilidad.Reserva
                WHERE TrayectoId = @trip AND UsuarioId = @user AND Estado = N'Confirmada') THEN 1
                WHEN EXISTS (SELECT 1 FROM movilidad.ColaEspera
                WHERE TrayectoId = @trip AND UsuarioId = @user AND Estado = N'EnEspera') THEN 2 ELSE 0 END;
            """, ("@trip", id), ("@user", userId));
        var participation = (int)(await existing.ExecuteScalarAsync(ct))!;
        if (participation == 1)
            return ApiError.Result(409, "RESERVA_DUPLICADA", "Ya tienes una plaza en este trayecto.");
        if (participation == 2)
            return ApiError.Result(409, "YA_EN_COLA", "Ya estás en la cola de este trayecto.");

        if (wait)
        {
            if (trip.Available > 0)
                return ApiError.Result(409, "PLAZAS_DISPONIBLES", "Hay plazas disponibles. Reserva directamente.");

            using var enqueue = Database.Command(connection, transaction, """
                INSERT movilidad.ColaEspera (TrayectoId, UsuarioId, Posicion)
                OUTPUT INSERTED.Posicion
                SELECT @trip, @user, COALESCE(MAX(Posicion), 0) + 1
                FROM movilidad.ColaEspera WHERE TrayectoId = @trip AND Estado = N'EnEspera';
                """, ("@trip", id), ("@user", userId));
            var position = (int)(await enqueue.ExecuteScalarAsync(ct))!;
            return Results.Json(new { trayectoId = id, posicion = position, estado = "EnEspera" }, statusCode: 201);
        }

        if (trip.Available == 0)
            return Results.Problem(statusCode: 409, title: "No quedan plazas. Usa la cola de espera.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "PLAZAS_AGOTADAS", ["listaEsperaDisponible"] = true
                });

        await ChangeAvailability(connection, transaction, id, -1, ct);
        var reservationId = await InsertReservation(connection, transaction, id, userId, ct);
        return Results.Json(new { id = reservationId, trayectoId = id, estado = "Confirmada" }, statusCode: 201);
    }, ct);

    private static Task<IResult> Cancel(long id, int userId, Database db, TimeProvider clock,
        CancellationToken ct) => db.Write<IResult>(async (connection, transaction) =>
    {
        using var lookup = Database.Command(connection, transaction, """
            SELECT TrayectoId FROM movilidad.Reserva WHERE Id = @id AND UsuarioId = @user;
            """, ("@id", id), ("@user", userId));
        if (await lookup.ExecuteScalarAsync(ct) is not int tripId)
            return ApiError.Result(404, "RESERVA_NO_ENCONTRADA", "Reserva no encontrada.");

        var trip = await LockTrip(connection, transaction, tripId, ct)
            ?? throw new InvalidOperationException("La reserva no tiene trayecto.");
        using var cancel = Database.Command(connection, transaction, """
            UPDATE movilidad.Reserva SET Estado = N'Cancelada'
            WHERE Id = @id AND Estado = N'Confirmada';
            """, ("@id", id));

        if (await cancel.ExecuteNonQueryAsync(ct) == 0)
            return Results.NoContent();

        var candidate = trip.Active && trip.Start > clock.GetUtcNow()
            ? await FirstWaiting(connection, transaction, tripId, ct)
            : null;
        if (candidate is null)
        {
            await ChangeAvailability(connection, transaction, tripId, 1, ct);
            return Results.NoContent();
        }

        await InsertReservation(connection, transaction, tripId, candidate.Value.UsuarioId, ct);
        using var promote = Database.Command(connection, transaction, """
            UPDATE movilidad.ColaEspera SET Estado = N'Promocionado' WHERE Id = @id;
            UPDATE movilidad.ColaEspera SET Posicion = Posicion - 1
                WHERE TrayectoId = @trip AND Estado = N'EnEspera' AND Posicion > @position;
            """, ("@id", candidate.Value.Id), ("@trip", tripId), ("@position", candidate.Value.Posicion));
        await promote.ExecuteNonQueryAsync(ct);
        return Results.NoContent();
    }, ct);

    private static async Task<Trip?> LockTrip(SqlConnection connection, SqlTransaction transaction, int id,
        CancellationToken ct)
    {
        using var command = Database.Command(connection, transaction, """
            SELECT ConductorId, PlazasDisponibles, Activo, Salida
            FROM movilidad.Trayecto WITH (UPDLOCK, HOLDLOCK) WHERE Id = @trip;
            """, ("@trip", id));
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        return await reader.ReadAsync(ct)
            ? new Trip(reader.GetInt32(0), reader.GetInt32(1), reader.GetBoolean(2),
                reader.GetFieldValue<DateTimeOffset>(3))
            : null;
    }

    private static async Task ChangeAvailability(SqlConnection connection, SqlTransaction transaction,
        int tripId, int delta, CancellationToken ct)
    {
        using var command = Database.Command(connection, transaction, """
            UPDATE movilidad.Trayecto SET PlazasDisponibles = PlazasDisponibles + @delta WHERE Id = @trip;
            """, ("@delta", delta), ("@trip", tripId));
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task<long> InsertReservation(SqlConnection connection, SqlTransaction transaction,
        int tripId, int userId, CancellationToken ct)
    {
        using var command = Database.Command(connection, transaction, """
            INSERT movilidad.Reserva (TrayectoId, UsuarioId) OUTPUT INSERTED.Id VALUES (@trip, @user);
            """, ("@trip", tripId), ("@user", userId));
        return (long)(await command.ExecuteScalarAsync(ct))!;
    }

    private static async Task<EntradaCola?> FirstWaiting(SqlConnection connection, SqlTransaction transaction,
        int tripId, CancellationToken ct)
    {
        using var command = Database.Command(connection, transaction, """
            SELECT Id, UsuarioId, Posicion FROM movilidad.ColaEspera
            WHERE TrayectoId = @trip AND Estado = N'EnEspera';
            """, ("@trip", tripId));
        await using var reader = await command.ExecuteReaderAsync(ct);
        var entries = new List<EntradaCola>();
        while (await reader.ReadAsync(ct))
            entries.Add(new EntradaCola(reader.GetInt64(0), reader.GetInt32(1), reader.GetInt32(2)));
        return ColaFifo.Primera(entries);
    }
}
