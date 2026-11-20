using TajamarApp.Core.DTOs.Auth;

namespace TajamarApp.Core.DTOs.Perfil;

public class PerfilMeResponseDto
{
    public UsuarioBasicoDto Usuario { get; set; } = null!;
    public bool PerfilCompletado { get; set; }
    public PerfilAppDto? PerfilApp { get; set; }
}
