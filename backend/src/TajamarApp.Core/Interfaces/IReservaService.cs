using TajamarApp.Core.DTOs.Reservas;

namespace TajamarApp.Core.Interfaces;

public interface IReservaService
{
    Task<IEnumerable<MiReservaDto>> GetMisReservasAsync(int idUsuario);
    Task<bool> CancelarReservaAsync(int idUsuario, int idReserva);
    Task<bool> CancelarColaAsync(int idUsuario, int idCola);
}
