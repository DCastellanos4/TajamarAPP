using TajamarApp.Core.DTOs.Common;
using TajamarApp.Core.DTOs.Trayectos;

namespace TajamarApp.Core.Interfaces;

public interface ITrayectoService
{
    Task<TrayectoDetalleDto> CrearTrayectoAsync(int idUsuario, CrearTrayectoDto dto);
    Task<TrayectoDetalleDto?> GetTrayectoPorIdAsync(int idTrayecto);
    Task<IEnumerable<TrayectoResumenDto>> GetMisTrayectosAsync(int idUsuario);
    Task<TrayectoDetalleDto?> EditarTrayectoAsync(int idUsuario, int idTrayecto, EditarTrayectoDto dto);
    Task<bool> CancelarTrayectoAsync(int idUsuario, int idTrayecto);
    Task<ResultadoPaginadoDto<TrayectoResumenDto>> BuscarTrayectosAsync(FiltroTrayectosDto filtro);
    Task<PasajerosTrayectoDto?> GetPasajerosTrayectoAsync(int idUsuarioConductor, int idTrayecto);
}
