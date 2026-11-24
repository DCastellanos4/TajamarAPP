namespace TajamarApp.Core.DTOs.Reservas;

public class MiReservaDto
{
    public int Id { get; set; }                      // ID de la reserva o de la entrada en cola
    public string TipoEntrada { get; set; } = null!; // "Reserva" o "ColaEspera"
    public int? IdReserva { get; set; }
    public int? IdCola { get; set; }
    public string Estado { get; set; } = null!;      // "Confirmada", "EnEspera", "Cancelada", "Promocionado"
    public int? PosicionCola { get; set; }           // 1, 2, 3... (si está en cola)
    public DateTime FechaRegistro { get; set; }

    // Datos del Trayecto
    public int TrayectoId { get; set; }
    public string Origen { get; set; } = null!;
    public string Destino { get; set; } = null!;
    public DateTime FechaHoraSalida { get; set; }
    public string TipoTrayecto { get; set; } = null!; // "Recurrente" / "Puntual"
    public string Modalidad { get; set; } = null!;    // "SoloIda", "SoloVuelta", "IdaYVuelta"
    public string? Turno { get; set; }               // "Manana" / "Tarde"
    public string? DiasSemana { get; set; }
    public int PlazasTotales { get; set; }
    public int PlazasDisponibles { get; set; }
    public string EstadoTrayecto { get; set; } = null!;
    public int? EventoId { get; set; }
    public string? EventoTitulo { get; set; }

    // Datos del Conductor
    public int ConductorIdUsuario { get; set; }
    public string ConductorNombre { get; set; } = null!;
    public string? ConductorFoto { get; set; }
    public string? ConductorTelefono { get; set; }
    public string? Coche { get; set; }                // "Marca Modelo"
    public string? ColorCoche { get; set; }
    public string? Matricula { get; set; }
}
