namespace TajamarApp.Core.DTOs.Auth;

public class UsuarioBasicoDto
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public string Rol { get; set; } = "ALUMNO";
}
