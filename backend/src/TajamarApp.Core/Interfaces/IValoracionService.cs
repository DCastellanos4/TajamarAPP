using TajamarApp.Core.DTOs.Valoraciones;

namespace TajamarApp.Core.Interfaces;

public interface IValoracionService
{
    Task<ResenaDto> CrearValoracionAsync(int idUsuarioPasajero, CrearValoracionDto dto);
    Task<ResumenResenasConductorDto> GetResenasConductorAsync(int idUsuarioConductor);
}
