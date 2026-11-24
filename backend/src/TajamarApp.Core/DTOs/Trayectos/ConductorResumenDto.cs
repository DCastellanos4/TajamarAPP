namespace TajamarApp.Core.DTOs.Trayectos;

public class ConductorResumenDto
{
    public int IdPerfil { get; set; }
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public string? Telefono { get; set; }
    public string? MarcaCoche { get; set; }
    public string? ModeloCoche { get; set; }
    public string? ColorCoche { get; set; }
    public string? Matricula { get; set; }
    public string? Bio { get; set; }
    public decimal? PuntuacionMedia { get; set; }
    public int TotalResenas { get; set; }
}
