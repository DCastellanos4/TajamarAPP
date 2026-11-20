namespace TajamarApp.Core.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public bool PerfilCompletado { get; set; }
    public UsuarioBasicoDto Usuario { get; set; } = null!;
}
