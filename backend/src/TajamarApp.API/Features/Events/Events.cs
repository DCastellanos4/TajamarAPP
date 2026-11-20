using System.Data;
using Microsoft.Data.SqlClient;
using TajamarApp.API.Auth;
using TajamarApp.API.Infrastructure;

namespace TajamarApp.API.Features.Events;

public sealed record EventInput(string? Nombre, DateTimeOffset? Inicio, string? Lugar, string? Imagen, string? Descripcion);

public static class Events
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
            SELECT (SELECT e.Id AS id, e.Titulo AS nombre, TODATETIMEOFFSET(e.FechaHora, '+00:00') AS inicio, e.Ubicacion AS lugar,
                e.Imagen AS imagen, CONVERT(bit, CASE WHEN e.FechaHora >= @now THEN 1 ELSE 0 END) AS activo,
                (SELECT COUNT(*) FROM dbo.EVENTO_ASISTENTES a WHERE a.EventoId = e.Id) AS totalAsistentes
            FROM dbo.EVENTOS_APP e WHERE e.FechaHora >= @now
            ORDER BY e.FechaHora, e.Id FOR JSON PATH);
            """, ("@now", clock.GetUtcNow().UtcDateTime));
        return Results.Content((string)(await command.ExecuteScalarAsync(ct) ?? "[]"), "application/json");
    }, ct);

    private static Task<IResult> Detail(int id, HttpContext http, Database db, TimeProvider clock, CancellationToken ct) =>
        db.Read<IResult>(async connection =>
        {
            var showAttendees = http.User.IsInRole("GestorEventos") || http.User.IsInRole("Administrador");
            using var command = Database.Command(connection, null, """
                SELECT (SELECT e.Id AS id, e.Titulo AS nombre, TODATETIMEOFFSET(e.FechaHora, '+00:00') AS inicio, e.Ubicacion AS lugar,
                    e.Imagen AS imagen, CONVERT(bit, CASE WHEN e.FechaHora >= @now THEN 1 ELSE 0 END) AS activo, e.Descripcion AS descripcion, e.CreadorId AS [organizador.id],
                    CONCAT(u.NOMBRE, N' ', u.APELLIDOS) AS [organizador.nombre],
                    (SELECT COUNT(*) FROM dbo.EVENTO_ASISTENTES a WHERE a.EventoId = e.Id) AS totalAsistentes,
                    JSON_QUERY(CASE WHEN @showAttendees = 1 THEN
                        (SELECT a.UsuarioTajamarId AS id, CONCAT(p.NOMBRE, N' ', p.APELLIDOS) AS nombre
                        FROM dbo.EVENTO_ASISTENTES a
                        JOIN dbo.USUARIOSTAJAMAR p ON p.IDUSUARIO = a.UsuarioTajamarId
                        WHERE a.EventoId = e.Id ORDER BY a.UsuarioTajamarId FOR JSON PATH) END) AS asistentes
                FROM dbo.EVENTOS_APP e JOIN dbo.USUARIOSTAJAMAR u ON u.IDUSUARIO = e.CreadorId WHERE e.Id = @id
                FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
                """, ("@id", id), ("@showAttendees", showAttendees), ("@now", clock.GetUtcNow().UtcDateTime));
            return await command.ExecuteScalarAsync(ct) is string { Length: > 0 } json
                ? Results.Content(json, "application/json")
                : Missing();
        }, ct);

    private static async Task<IResult> Save(int? id, EventInput input, HttpContext http, Database db,
        TimeProvider clock, CancellationToken ct)
    {
        var name = input.Nombre?.Trim();
        var location = input.Lugar?.Trim();
        var description = input.Descripcion?.Trim() ?? string.Empty;
        var image = string.IsNullOrWhiteSpace(input.Imagen) ? null : input.Imagen.Trim();
        if (name is not { Length: > 0 and <= 150 } || location is not { Length: > 0 and <= 200 } ||
            input.Inicio is not DateTimeOffset start || image is { Length: > 600 } || description.Length > 2000)
            return ApiError.Result(400, "EVENTO_INVALIDO", "Revisa el nombre, el lugar, la fecha y la imagen.");

        if (TimeZoneInfo.ConvertTime(start, Madrid).Date < TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Madrid).Date)
            return ApiError.Result(400, "FECHA_EVENTO_PASADA", "La fecha del evento es anterior a hoy.");

        return await db.Write<IResult>(async (connection, transaction) =>
        {
            var userId = TeamJwt.UserId(http.User);
            var sql = id.HasValue
                ? """
                  UPDATE dbo.EVENTOS_APP SET Titulo = @nombre, FechaHora = @inicio, Ubicacion = @lugar,
                      Imagen = @imagen, Descripcion = @descripcion
                  WHERE Id = @id AND (@admin = 1 OR CreadorId = @usuario);
                  """
                : """
                  INSERT dbo.EVENTOS_APP (Titulo, FechaHora, Ubicacion, Imagen, CreadorId, Descripcion, FechaCreacion)
                  OUTPUT INSERTED.Id VALUES (@nombre, @inicio, @lugar, @imagen, @usuario, @descripcion, SYSUTCDATETIME());
                  """;
            using var command = Database.Command(connection, transaction, sql,
                ("@nombre", name), ("@inicio", start.UtcDateTime), ("@lugar", location), ("@imagen", image),
                ("@usuario", userId), ("@descripcion", description), ("@id", id),
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
            SELECT FechaHora FROM dbo.EVENTOS_APP WITH (UPDLOCK, HOLDLOCK) WHERE Id = @id;
            """, ("@id", id));
        await using (var reader = await lookup.ExecuteReaderAsync(CommandBehavior.SingleRow, ct))
        {
            if (!await reader.ReadAsync(ct))
                return Missing();
            if (reader.GetDateTime(0) <= clock.GetUtcNow().UtcDateTime)
                return ApiError.Result(409, "EVENTO_CERRADO", "El evento ya no admite inscripciones.");
        }

        var userId = TeamJwt.UserId(http.User);
        using var command = Database.Command(connection, transaction, """
            INSERT dbo.EVENTO_ASISTENTES (EventoId, UsuarioTajamarId, FechaInscripcion)
            SELECT @id, @usuario, SYSUTCDATETIME() WHERE NOT EXISTS (
                SELECT 1 FROM dbo.EVENTO_ASISTENTES WHERE EventoId = @id AND UsuarioTajamarId = @usuario);
            """, ("@id", id), ("@usuario", userId));
        return await command.ExecuteNonQueryAsync(ct) == 1
            ? Results.StatusCode(201)
            : ApiError.Result(409, "ASISTENCIA_DUPLICADA", "Ya estás inscrito en este evento.");
    }, ct);

    private static Task<IResult> Cancel(int id, HttpContext http, Database db, CancellationToken ct) =>
        db.Write<IResult>(async (connection, transaction) =>
        {
            using var lookup = Database.Command(connection, transaction,
                "SELECT Id FROM dbo.EVENTOS_APP WITH (UPDLOCK, HOLDLOCK) WHERE Id = @id;", ("@id", id));
            if (await lookup.ExecuteScalarAsync(ct) is null)
                return Missing();

            using var command = Database.Command(connection, transaction,
                "DELETE dbo.EVENTO_ASISTENTES WHERE EventoId = @id AND UsuarioTajamarId = @usuario;",
                ("@id", id), ("@usuario", TeamJwt.UserId(http.User)));
            await command.ExecuteNonQueryAsync(ct);
            return Results.NoContent();
        }, ct);

    private static IResult Missing() => ApiError.Result(404, "EVENTO_NO_ENCONTRADO", "Evento no encontrado.");
}
