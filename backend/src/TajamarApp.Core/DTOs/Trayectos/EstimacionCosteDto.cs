namespace TajamarApp.Core.DTOs.Trayectos;

public class EstimacionCosteDto
{
    public double DistanciaKm { get; set; }
    public decimal CosteTotalTrayecto { get; set; }
    public int PlazasOcupadas { get; set; }
    public decimal CostePorPlaza { get; set; }
    public string ConceptoBizum { get; set; } = string.Empty;
    public string? TelefonoBizum { get; set; }
    public decimal AhorroEstimadoVsTaxi { get; set; }
    public decimal Co2AhorradoKg { get; set; }
}
