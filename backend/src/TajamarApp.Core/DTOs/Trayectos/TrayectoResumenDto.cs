namespace TajamarApp.Core.DTOs.Trayectos;

public class TrayectoResumenDto
{
    public int Id { get; set; }
    public string ConductorNombre { get; set; } = string.Empty;
    public string? ConductorImagen { get; set; }
    public string? Coche { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public DateTime FechaHoraSalida { get; set; }
    public int PlazasDisponibles { get; set; }
    public int PlazasTotales { get; set; }
    public string TipoTrayecto { get; set; } = "Recurrente";
    public string Modalidad { get; set; } = "IdaYVuelta";
    public string? Turno { get; set; }
    public string? DiasSemana { get; set; }
    public string Estado { get; set; } = "Activo";
    public int? EventoId { get; set; }
    public decimal? ConductorPuntuacionMedia { get; set; }
    public int ConductorTotalResenas { get; set; }
}
