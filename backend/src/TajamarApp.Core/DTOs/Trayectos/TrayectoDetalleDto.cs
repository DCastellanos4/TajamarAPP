namespace TajamarApp.Core.DTOs.Trayectos;

public class TrayectoDetalleDto
{
    public int Id { get; set; }
    public ConductorResumenDto Conductor { get; set; } = null!;
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public DateTime FechaHoraSalida { get; set; }
    public int PlazasTotales { get; set; }
    public int PlazasDisponibles { get; set; }
    public string TipoTrayecto { get; set; } = "Recurrente";
    public string Modalidad { get; set; } = "IdaYVuelta";
    public string? DiasSemana { get; set; }
    public string? Turno { get; set; }
    public string Estado { get; set; } = "Activo";
    public int? EventoId { get; set; }
    public string? EventoTitulo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
