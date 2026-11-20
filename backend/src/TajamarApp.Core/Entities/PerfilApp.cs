namespace TajamarApp.Core.Entities;

public class PerfilApp
{
    public int Id { get; set; }
    public int IdUsuarioTajamar { get; set; }
    public string? Telefono { get; set; }
    public bool EsConductor { get; set; }
    public string? MarcaCoche { get; set; }
    public string? ModeloCoche { get; set; }
    public string? ColorCoche { get; set; }
    public string? Matricula { get; set; }
    public int PlazasCoche { get; set; }
    public string? Bio { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public UsuarioTajamar UsuarioTajamar { get; set; } = null!;
    public ICollection<Trayecto> TrayectosPublicados { get; set; } = new List<Trayecto>();
}
