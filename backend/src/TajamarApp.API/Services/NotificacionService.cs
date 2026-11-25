using Microsoft.AspNetCore.SignalR;
using TajamarApp.API.Hubs;
using TajamarApp.Core.DTOs.Notificaciones;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.API.Services;

public class NotificacionService : INotificacionService
{
    private readonly IHubContext<NotificacionesHub, INotificacionClient> _hubContext;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        IHubContext<NotificacionesHub, INotificacionClient> hubContext,
        ILogger<NotificacionService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotificarNuevaReservaAsync(int idConductorUsuario, int trayectoId, string nombrePasajero, string origen, string destino)
    {
        var notificacion = new NotificacionDto
        {
            Tipo = "NUEVA_RESERVA",
            Titulo = "¡Nueva plaza reservada!",
            Mensaje = $"{nombrePasajero} ha reservado una plaza en tu trayecto de {origen} a {destino}.",
            Fecha = DateTime.UtcNow,
            Datos = new Dictionary<string, object?>
            {
                ["trayectoId"] = trayectoId,
                ["nombrePasajero"] = nombrePasajero,
                ["origen"] = origen,
                ["destino"] = destino
            }
        };

        _logger.LogInformation("SignalR: Enviando 'NuevaReserva' a Conductor Usuario {IdUsuario}", idConductorUsuario);

        var target = _hubContext.Clients.User(idConductorUsuario.ToString());
        await Task.WhenAll(
            target.RecibirNotificacion(notificacion),
            target.NuevaReserva(notificacion)
        );
    }

    public async Task NotificarPromocionColaAsync(int idPasajeroUsuario, int trayectoId, string origen, string destino)
    {
        var notificacion = new NotificacionDto
        {
            Tipo = "PROMOCION_COLA",
            Titulo = "¡Plaza confirmada!",
            Mensaje = $"¡Buenas noticias! Se ha liberado una plaza y se ha confirmado tu reserva en el trayecto de {origen} a {destino}.",
            Fecha = DateTime.UtcNow,
            Datos = new Dictionary<string, object?>
            {
                ["trayectoId"] = trayectoId,
                ["origen"] = origen,
                ["destino"] = destino
            }
        };

        _logger.LogInformation("SignalR: Enviando 'PromocionCola' a Alumno Usuario {IdUsuario}", idPasajeroUsuario);

        var target = _hubContext.Clients.User(idPasajeroUsuario.ToString());
        await Task.WhenAll(
            target.RecibirNotificacion(notificacion),
            target.PromocionCola(notificacion)
        );
    }

    public async Task NotificarTrayectoCanceladoAsync(IEnumerable<int> idsPasajerosUsuarios, int trayectoId, string origen, string destino)
    {
        var notificacion = new NotificacionDto
        {
            Tipo = "TRAYECTO_CANCELADO",
            Titulo = "Trayecto cancelado",
            Mensaje = $"El conductor ha cancelado el trayecto de {origen} a {destino}.",
            Fecha = DateTime.UtcNow,
            Datos = new Dictionary<string, object?>
            {
                ["trayectoId"] = trayectoId,
                ["origen"] = origen,
                ["destino"] = destino
            }
        };

        var ids = idsPasajerosUsuarios.Distinct().ToList();
        _logger.LogInformation("SignalR: Enviando 'TrayectoCancelado' a {Count} pasajeros del trayecto {TrayectoId}", ids.Count, trayectoId);

        foreach (var id in ids)
        {
            var target = _hubContext.Clients.User(id.ToString());
            await Task.WhenAll(
                target.RecibirNotificacion(notificacion),
                target.TrayectoCancelado(notificacion)
            );
        }
    }
}
