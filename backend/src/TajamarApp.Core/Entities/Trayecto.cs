namespace TajamarApp.Core.Entities;

public class Trayecto
{
    public int Id { get; set; }
    public int ConductorPerfilId { get; set; }
    public int? EventoId { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = "Tajamar";
    public DateTime FechaHoraSalida { get; set; }
    public int PlazasTotales { get; set; }
    public int PlazasDisponibles { get; set; }
    public string TipoTrayecto { get; set; } = "Recurrente"; // Recurrente / Puntual
    public string Modalidad { get; set; } = "IdaYVuelta";   // SoloIda / SoloVuelta / IdaYVuelta
    public string? DiasSemana { get; set; }                 // Ej: "L,M,X,J,V"
    public string? Turno { get; set; }                      // Manana / Tarde
    public string Estado { get; set; } = "Activo";          // Activo / Completo / Cancelado
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public PerfilApp Conductor { get; set; } = null!;
    public EventoApp? Evento { get; set; }
    public ICollection<ReservaTrayecto> Reservas { get; set; } = new List<ReservaTrayecto>();
    public ICollection<ColaEspera> ColaEspera { get; set; } = new List<ColaEspera>();
}
