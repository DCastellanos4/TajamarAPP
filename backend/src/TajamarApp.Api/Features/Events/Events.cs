using System.Data;
using Microsoft.Data.SqlClient;

sealed record EventInput(string? Nombre, DateTimeOffset? Inicio, string? Lugar, string? Imagen);

static class Events
{
    private static readonly TimeZoneInfo Madrid = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

    public static void MapEvents(this RouteGroupBuilder api)
    {
        var routes = api.MapGroup("/eventos");
        routes.MapGet("", List);
        routes.MapGet("/{id:int}", Detail).WithName("DetalleEvento");
        routes.MapPost("", (EventInput input, HttpContext http, Database db, TimeProvider clock, CancellationToken ct) =>
            Save(null, input, http, db, clock, ct)).RequireAuthorization("GestionEventos");
        routes.MapPut("/{id:int}", (int id, EventInput input, HttpContext http, Database db, TimeProvider clock,
            CancellationToken ct) => Save(id, input, http, db, clock, ct)).RequireAuthorization("GestionEventos");
        routes.MapPost("/{id:int}/asistir", Register).RequireAuthorization();
        routes.MapDelete("/{id:int}/asistir", Cancel).RequireAuthorization();
    }

    private static Task<IResult> List(Database db, TimeProvider clock, CancellationToken ct) => db.Read<IResult>(async connection =>
    {
        using var command = Database.Command(connection, null, """
            SELECT (SELECT e.Id AS id, e.Nombre AS nombre, e.Inicio AS inicio, e.Lugar AS lugar,
                e.Imagen AS imagen, e.Activo AS activo,
                (SELECT COUNT(*) FROM movilidad.Asistencia a WHERE a.EventoId = e.Id) AS totalAsistentes
            FROM movilidad.Evento e WHERE e.Activo = 1 AND e.Inicio >= @now
            ORDER BY e.Inicio, e.Id FOR JSON PATH);
            """, ("@now", clock.GetUtcNow()));
        return Results.Content((string)(await command.ExecuteScalarAsync(ct) ?? "[]"), "application/json");
    }, ct);

    private static Task<IResult> Detail(int id, HttpContext http, Database db, CancellationToken ct) =>
        db.Read<IResult>(async connection =>
        {
            var showAttendees = http.User.IsInRole("GestorEventos") || http.User.IsInRole("Administrador");
            using var command = Database.Command(connection, null, """
                SELECT (SELECT e.Id AS id, e.Nombre AS nombre, e.Inicio AS inicio, e.Lugar AS lugar,
                    e.Imagen AS imagen, e.Activo AS activo, e.OrganizadorId AS [organizador.id],
                    e.OrganizadorNombre AS [organizador.nombre],
                    (SELECT COUNT(*) FROM movilidad.Asistencia a WHERE a.EventoId = e.Id) AS totalAsistentes,
                    JSON_QUERY(CASE WHEN @showAttendees = 1 THEN
                        (SELECT a.UsuarioId AS id, a.UsuarioNombre AS nombre FROM movilidad.Asistencia a
                        WHERE a.EventoId = e.Id ORDER BY a.UsuarioId FOR JSON PATH) END) AS asistentes
                FROM movilidad.Evento e WHERE e.Id = @id
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
                """, ("@id", id), ("@showAttendees", showAttendees));
            return await command.ExecuteScalarAsync(ct) is string { Length: > 0 } json
                ? Results.Content(json, "application/json")
                : Missing();
        }, ct);

    private static async Task<IResult> Save(int? id, EventInput input, HttpContext http, Database db,
        TimeProvider clock, CancellationToken ct)
    {
        var name = input.Nombre?.Trim();
        var location = input.Lugar?.Trim();
        var image = string.IsNullOrWhiteSpace(input.Imagen) ? null : input.Imagen.Trim();
        if (name is not { Length: > 0 and <= 150 } || location is not { Length: > 0 and <= 250 } ||
            input.Inicio is not DateTimeOffset start || image is { Length: > 2048 })
            return ApiError.Result(400, "EVENTO_INVALIDO", "Revisa el nombre, el lugar, la fecha y la imagen.");

        if (TimeZoneInfo.ConvertTime(start, Madrid).Date < TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Madrid).Date)
            return ApiError.Result(400, "FECHA_EVENTO_PASADA", "La fecha del evento es anterior a hoy.");

        return await db.Write<IResult>(async (connection, transaction) =>
        {
            var userId = TeamJwt.UserId(http.User);
            var sql = id.HasValue
                ? """
                  UPDATE movilidad.Evento SET Nombre = @nombre, Inicio = @inicio, Lugar = @lugar, Imagen = @imagen
                  WHERE Id = @id AND (@admin = 1 OR OrganizadorId = @usuario);
                  """
                : """
                  INSERT movilidad.Evento (Nombre, Inicio, Lugar, Imagen, OrganizadorId, OrganizadorNombre)
                  OUTPUT INSERTED.Id VALUES (@nombre, @inicio, @lugar, @imagen, @usuario, @organizador);
                  """;
            using var command = Database.Command(connection, transaction, sql,
                ("@nombre", name), ("@inicio", start), ("@lugar", location), ("@imagen", image),
                ("@usuario", userId), ("@organizador", DisplayName(http, userId)), ("@id", id),
                ("@admin", http.User.IsInRole("Administrador")));
            if (id.HasValue)
                return await command.ExecuteNonQueryAsync(ct) == 1 ? Results.NoContent() : Missing();

            var newId = (int)(await command.ExecuteScalarAsync(ct))!;
            return Results.CreatedAtRoute("DetalleEvento", new { id = newId }, new { id = newId });
        }, ct);
    }

    private static Task<IResult> Register(int id, HttpContext http, Database db, TimeProvider clock,
        CancellationToken ct) => db.Write<IResult>(async (connection, transaction) =>
    {
        using var lookup = Database.Command(connection, transaction, """
            SELECT Activo, Inicio FROM movilidad.Evento WITH (UPDLOCK, HOLDLOCK) WHERE Id = @id;
            """, ("@id", id));
        await using (var reader = await lookup.ExecuteReaderAsync(CommandBehavior.SingleRow, ct))
        {
            if (!await reader.ReadAsync(ct))
                return Missing();
            if (!reader.GetBoolean(0) || reader.GetFieldValue<DateTimeOffset>(1) <= clock.GetUtcNow())
                return ApiError.Result(409, "EVENTO_CERRADO", "El evento ya no admite inscripciones.");
        }

        var userId = TeamJwt.UserId(http.User);
        using var command = Database.Command(connection, transaction, """
            INSERT movilidad.Asistencia (EventoId, UsuarioId, UsuarioNombre)
            SELECT @id, @usuario, @nombre WHERE NOT EXISTS (
                SELECT 1 FROM movilidad.Asistencia WHERE EventoId = @id AND UsuarioId = @usuario);
            """, ("@id", id), ("@usuario", userId), ("@nombre", DisplayName(http, userId)));
        return await command.ExecuteNonQueryAsync(ct) == 1
            ? Results.StatusCode(201)
            : ApiError.Result(409, "ASISTENCIA_DUPLICADA", "Ya estás inscrito en este evento.");
    }, ct);

    private static Task<IResult> Cancel(int id, HttpContext http, Database db, CancellationToken ct) =>
        db.Write<IResult>(async (connection, transaction) =>
        {
            using var lookup = Database.Command(connection, transaction,
                "SELECT Id FROM movilidad.Evento WITH (UPDLOCK, HOLDLOCK) WHERE Id = @id;", ("@id", id));
            if (await lookup.ExecuteScalarAsync(ct) is null)
                return Missing();

            using var command = Database.Command(connection, transaction,
                "DELETE movilidad.Asistencia WHERE EventoId = @id AND UsuarioId = @usuario;",
                ("@id", id), ("@usuario", TeamJwt.UserId(http.User)));
            await command.ExecuteNonQueryAsync(ct);
            return Results.NoContent();
        }, ct);

    private static string DisplayName(HttpContext http, int userId)
    {
        var name = http.User.Identity?.Name?.Trim();
        return string.IsNullOrEmpty(name) ? $"Usuario {userId}" : name[..Math.Min(name.Length, 150)];
    }

    private static IResult Missing() => ApiError.Result(404, "EVENTO_NO_ENCONTRADO", "Evento no encontrado.");
}
