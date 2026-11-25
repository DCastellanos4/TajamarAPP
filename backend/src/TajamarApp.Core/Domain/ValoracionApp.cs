using TajamarApp.Core.Entities;

namespace TajamarApp.Core.Domain;

public class ValoracionApp
{
    public int Id { get; set; }
    public int TrayectoId { get; set; }
    public int ConductorPerfilId { get; set; }
    public int PasajeroUsuarioId { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public string? Tags { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones de navegación
    public Trayecto Trayecto { get; set; } = null!;
    public PerfilApp Conductor { get; set; } = null!;
    public UsuarioTajamar Pasajero { get; set; } = null!;
}
