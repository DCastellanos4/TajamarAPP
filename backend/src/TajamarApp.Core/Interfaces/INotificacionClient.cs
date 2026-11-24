using TajamarApp.Core.DTOs.Notificaciones;

namespace TajamarApp.Core.Interfaces;

public interface INotificacionClient
{
    Task RecibirNotificacion(NotificacionDto notificacion);
    Task NuevaReserva(NotificacionDto notificacion);
    Task PromocionCola(NotificacionDto notificacion);
    Task TrayectoCancelado(NotificacionDto notificacion);
}
