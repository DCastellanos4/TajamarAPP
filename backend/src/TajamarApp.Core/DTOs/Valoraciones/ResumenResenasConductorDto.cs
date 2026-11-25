namespace TajamarApp.Core.DTOs.Valoraciones;

public class ResumenResenasConductorDto
{
    public int ConductorIdUsuario { get; set; }
    public int ConductorPerfilId { get; set; }
    public string ConductorNombre { get; set; } = string.Empty;
    public string? ConductorFoto { get; set; }
    public decimal PuntuacionMedia { get; set; }
    public int TotalResenas { get; set; }
    public Dictionary<int, int> DesgloseEstrellas { get; set; } = new()
    {
        { 1, 0 },
        { 2, 0 },
        { 3, 0 },
        { 4, 0 },
        { 5, 0 }
    };
    public List<ResenaDto> Resenas { get; set; } = new();
}
