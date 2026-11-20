namespace TajamarApp.Core.Entities;

public class RolTajamar
{
    public int IdRole { get; set; }
    public string Role { get; set; } = string.Empty;

    // Relaciones
    public ICollection<UsuarioTajamar> Usuarios { get; set; } = new List<UsuarioTajamar>();
}
