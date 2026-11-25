namespace TajamarApp.Core.Entities;

public class EventoApp
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string Ubicacion { get; set; } = "Tajamar";
    public string? Imagen { get; set; }
    public int CreadorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public UsuarioTajamar Creador { get; set; } = null!;
    public ICollection<EventoAsistente> Asistentes { get; set; } = new List<EventoAsistente>();
    public ICollection<Trayecto> Trayectos { get; set; } = new List<Trayecto>();
}
