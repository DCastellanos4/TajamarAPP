using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

Check(ColaFifo.Primera([]) is null, "Una cola vacía no tiene candidato.");
EntradaCola[] waiting = [new(50, 10, 3), new(20, 11, 2), new(30, 12, 1)];
Check(ColaFifo.Primera(waiting)?.UsuarioId == 12, "Se promociona primero la posición 1.");
Check(ColaFifo.Primera(waiting.Where(x => x.UsuarioId != 12))?.UsuarioId == 11, "Se respeta el turno siguiente.");
Check(ColaFifo.Primera([new(99, 1, 1), new(98, 2, 1)])?.UsuarioId == 2, "El desempate usa el orden de alta.");
Console.WriteLine("Pruebas unitarias FIFO superadas.");

if (!args.Contains("--sql", StringComparer.Ordinal))
    return;

var connectionString = Environment.GetEnvironmentVariable("TAJAMAR_TEST_SQL")
    ?? throw new InvalidOperationException("Configura TAJAMAR_TEST_SQL con una base de datos de pruebas.");
var signingKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
await using var app = TajamarApplication.Build([], builder =>
{
    builder.Logging.ClearProviders();
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:Tajamar"] = connectionString,
        ["Jwt:SigningKey"] = signingKey,
        ["Jwt:Issuer"] = "tajamar-tests",
        ["Jwt:Audience"] = "tajamar-tests"
    });
});
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
using var anonymous = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

using var driver = Client(900001, "GestorEventos");
using var first = Client(900002);
using var second = Client(900003);
using var third = Client(900004);

await using var sql = new SqlConnection(connectionString);
await sql.OpenAsync();
int? tripId = null;
int? eventId = null;
try
{
    using var setup = new SqlCommand("""
        INSERT movilidad.Trayecto (ConductorId, Salida, Capacidad, PlazasDisponibles)
        OUTPUT INSERTED.Id VALUES (900001, DATEADD(day, 1, SYSDATETIMEOFFSET()), 1, 1);
        """, sql);
    tripId = (int)(await setup.ExecuteScalarAsync())!;
    var tripPath = $"/api/trayectos/{tripId}";

    await Expect(anonymous.PostAsync(tripPath + "/reservar", null), HttpStatusCode.Unauthorized);
    await Expect(driver.PostAsync(tripPath + "/reservar", null), HttpStatusCode.Conflict, "TRAYECTO_PROPIO");

    using var otherConnection = new SqlConnection(connectionString);
    await otherConnection.OpenAsync();
    await using var gate = (SqlTransaction)await otherConnection.BeginTransactionAsync();
    using var blockTrip = new SqlCommand("""
        SELECT Id FROM movilidad.Trayecto WITH (UPDLOCK, HOLDLOCK) WHERE Id = @id;
        """, otherConnection, gate);
    blockTrip.Parameters.AddWithValue("@id", tripId.Value);
    await blockTrip.ExecuteScalarAsync();

    var pending = new[] { first.PostAsync(tripPath + "/reservar", null), second.PostAsync(tripPath + "/reservar", null) };
    await Task.Delay(100);
    await gate.CommitAsync();
    var race = await Task.WhenAll(pending);
    using var raceFirst = race[0];
    using var raceSecond = race[1];
    Check(race.Count(r => r.StatusCode == HttpStatusCode.Created) == 1 &&
          race.Count(r => r.StatusCode == HttpStatusCode.Conflict) == 1, "Solo una petición obtiene la última plaza.");
    var winningIndex = Array.FindIndex(race, r => r.StatusCode == HttpStatusCode.Created);
    var winner = winningIndex == 0 ? first : second;
    var loser = winningIndex == 0 ? second : first;
    var loserId = winningIndex == 0 ? 900003 : 900002;
    var winnerReservation = (await Body(race[winningIndex])).GetProperty("id").GetInt64();
    Check((await Body(race[1 - winningIndex])).GetProperty("code").GetString() == "PLAZAS_AGOTADAS",
        "Se indica la cola al perder la última plaza.");
    Check(await Available() == 0 && (await Confirmed()).Count == 1, "SQL guarda una reserva y cero plazas.");

    await Expect(loser.DeleteAsync($"/api/reservas/{winnerReservation}"), HttpStatusCode.NotFound);
    Check((await Expect(loser.PostAsync(tripPath + "/cola", null), HttpStatusCode.Created))
        .GetProperty("posicion").GetInt32() == 1, "El primer pasajero ocupa la posición 1.");
    await Expect(loser.PostAsync(tripPath + "/cola", null), HttpStatusCode.Conflict, "YA_EN_COLA");
    Check((await Expect(third.PostAsync(tripPath + "/cola", null), HttpStatusCode.Created))
        .GetProperty("posicion").GetInt32() == 2, "El segundo pasajero ocupa la posición 2.");

    await Expect(winner.DeleteAsync($"/api/reservas/{winnerReservation}"), HttpStatusCode.NoContent);
    await Expect(winner.DeleteAsync($"/api/reservas/{winnerReservation}"), HttpStatusCode.NoContent);
    var promoted = (await Confirmed()).Single();
    Check(promoted.UserId == loserId && await Available() == 0, "El primer pasajero recibe la plaza sin liberar inventario.");
    using var checkQueue = new SqlCommand("""
        SELECT COUNT(*) FROM movilidad.ColaEspera WHERE TrayectoId = @id AND
        ((UsuarioId = @first AND Estado = N'Promocionado') OR
         (UsuarioId = 900004 AND Estado = N'EnEspera' AND Posicion = 1));
        """, sql);
    checkQueue.Parameters.AddWithValue("@id", tripId.Value);
    checkQueue.Parameters.AddWithValue("@first", loserId);
    Check((int)(await checkQueue.ExecuteScalarAsync())! == 2, "Se actualizan el estado y la posición de la cola.");

    await Expect(loser.DeleteAsync($"/api/reservas/{promoted.Id}"), HttpStatusCode.NoContent);
    var finalReservation = (await Confirmed()).Single();
    Check(finalReservation.UserId == 900004, "El segundo pasajero se promociona en orden.");
    await Expect(third.DeleteAsync($"/api/reservas/{finalReservation.Id}"), HttpStatusCode.NoContent);
    Check(await Available() == 1 && (await Confirmed()).Count == 0, "Con la cola vacía se restituye la plaza.");

    var start = DateTimeOffset.UtcNow.AddDays(2);
    var draft = new { nombre = "Evento de integración", inicio = start, lugar = "Tajamar" };
    await Expect(first.PostAsJsonAsync("/api/eventos", draft), HttpStatusCode.Forbidden);
    await Expect(driver.PostAsJsonAsync("/api/eventos", draft with { inicio = start.AddDays(-4) }),
        HttpStatusCode.BadRequest, "FECHA_EVENTO_PASADA");
    eventId = (await Expect(driver.PostAsJsonAsync("/api/eventos", draft), HttpStatusCode.Created))
        .GetProperty("id").GetInt32();
    var eventPath = $"/api/eventos/{eventId}";
    await Expect(driver.PutAsJsonAsync(eventPath, draft with { nombre = "Evento actualizado" }), HttpStatusCode.NoContent);
    var events = await Expect(anonymous.GetAsync("/api/eventos"), HttpStatusCode.OK);
    Check(events.EnumerateArray().Any(e => e.GetProperty("id").GetInt32() == eventId), "El listado incluye el evento activo.");
    var attend = await Task.WhenAll(first.PostAsync(eventPath + "/asistir", null), first.PostAsync(eventPath + "/asistir", null));
    using var attendFirst = attend[0];
    using var attendSecond = attend[1];
    Check(attend.Count(r => r.StatusCode == HttpStatusCode.Created) == 1 &&
          attend.Count(r => r.StatusCode == HttpStatusCode.Conflict) == 1, "La asistencia concurrente es única.");
    var detail = await Expect(anonymous.GetAsync(eventPath), HttpStatusCode.OK);
    Check(detail.GetProperty("totalAsistentes").GetInt32() == 1 &&
          detail.GetProperty("organizador").GetProperty("id").GetInt32() == 900001 &&
          !detail.TryGetProperty("asistentes", out _), "El detalle público incluye organizador y contador.");
    var managedDetail = await Expect(driver.GetAsync(eventPath), HttpStatusCode.OK);
    Check(managedDetail.GetProperty("asistentes").GetArrayLength() == 1, "El gestor autorizado ve los asistentes.");
    await Expect(first.DeleteAsync(eventPath + "/asistir"), HttpStatusCode.NoContent);
    detail = await Expect(anonymous.GetAsync(eventPath), HttpStatusCode.OK);
    Check(detail.GetProperty("totalAsistentes").GetInt32() == 0, "Cancelar actualiza el contador.");
    Console.WriteLine("Pruebas de integración SQL Server superadas.");
}
finally
{
    if (tripId.HasValue || eventId.HasValue)
    {
        using var cleanup = new SqlCommand("""
            DELETE movilidad.ColaEspera WHERE TrayectoId = @trip;
            DELETE movilidad.Reserva WHERE TrayectoId = @trip;
            DELETE movilidad.Trayecto WHERE Id = @trip;
            DELETE movilidad.Asistencia WHERE EventoId = @event;
            DELETE movilidad.Evento WHERE Id = @event;
            """, sql);
        cleanup.Parameters.AddWithValue("@trip", (object?)tripId ?? DBNull.Value);
        cleanup.Parameters.AddWithValue("@event", (object?)eventId ?? DBNull.Value);
        await cleanup.ExecuteNonQueryAsync();
    }
}

async Task<int> Available()
{
    using var command = new SqlCommand("SELECT PlazasDisponibles FROM movilidad.Trayecto WHERE Id = @trip;", sql);
    command.Parameters.AddWithValue("@trip", tripId!.Value);
    return (int)(await command.ExecuteScalarAsync())!;
}

async Task<List<(long Id, int UserId)>> Confirmed()
{
    using var command = new SqlCommand("""
        SELECT Id, UsuarioId FROM movilidad.Reserva WHERE TrayectoId = @trip AND Estado = N'Confirmada';
        """, sql);
    command.Parameters.AddWithValue("@trip", tripId!.Value);
    await using var rows = await command.ExecuteReaderAsync();
    var result = new List<(long, int)>();
    while (await rows.ReadAsync())
        result.Add((rows.GetInt64(0), rows.GetInt32(1)));
    return result;
}

HttpClient Client(int id, string? role = null)
{
    List<Claim> claims = [new("idUsuarioTajamar", id.ToString()), new("nombre", $"Demo {id}")];
    if (role is not null)
        claims.Add(new Claim("role", role));
    var credentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKey)),
        SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken("tajamar-tests", "tajamar-tests", claims, expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: credentials);
    var client = new HttpClient { BaseAddress = anonymous.BaseAddress };
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
    return client;
}

static async Task<JsonElement> Expect(Task<HttpResponseMessage> request, HttpStatusCode status, string? code = null)
{
    using var response = await request;
    var content = await response.Content.ReadAsStringAsync();
    Check(response.StatusCode == status, $"Se esperaba {status}; recibido {response.StatusCode}. {content}");
    if (string.IsNullOrWhiteSpace(content))
        return default;
    using var document = JsonDocument.Parse(content);
    if (code is not null)
        Check(document.RootElement.GetProperty("code").GetString() == code, $"Se esperaba el código {code}.");
    return document.RootElement.Clone();
}

static async Task<JsonElement> Body(HttpResponseMessage response)
{
    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    return json.RootElement.Clone();
}

static void Check(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
