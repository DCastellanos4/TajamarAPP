using TajamarApp.Core.DTOs.Perfil;

namespace TajamarApp.Core.Interfaces;

public interface IPerfilService
{
    Task<PerfilMeResponseDto?> GetMiPerfilAsync(int idUsuario);
    Task<PerfilAppDto> GuardarPerfilAsync(int idUsuario, PerfilAppDto dto);
}
