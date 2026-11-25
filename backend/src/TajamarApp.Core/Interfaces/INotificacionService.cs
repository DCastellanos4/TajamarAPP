using TajamarApp.Core.DTOs.Notificaciones;

namespace TajamarApp.Core.Interfaces;

public interface INotificacionService
{
    Task NotificarNuevaReservaAsync(int idConductorUsuario, int trayectoId, string nombrePasajero, string origen, string destino);
    Task NotificarPromocionColaAsync(int idPasajeroUsuario, int trayectoId, string origen, string destino);
    Task NotificarTrayectoCanceladoAsync(IEnumerable<int> idsPasajerosUsuarios, int trayectoId, string origen, string destino);
}
