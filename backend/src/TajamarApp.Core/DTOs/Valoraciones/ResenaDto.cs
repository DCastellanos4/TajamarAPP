namespace TajamarApp.Core.DTOs.Valoraciones;

public class ResenaDto
{
    public int Id { get; set; }
    public int TrayectoId { get; set; }
    public int ConductorPerfilId { get; set; }
    public int PasajeroUsuarioId { get; set; }
    public string PasajeroNombre { get; set; } = string.Empty;
    public string? PasajeroFoto { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public string? Tags { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? OrigenTrayecto { get; set; }
    public string? DestinoTrayecto { get; set; }
}
