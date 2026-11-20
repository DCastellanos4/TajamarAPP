using System.ComponentModel.DataAnnotations;

namespace TajamarApp.Core.DTOs.Trayectos;

public class CrearTrayectoDto
{
    [Required(ErrorMessage = "El punto de origen es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El origen no puede exceder 150 caracteres.")]
    public string Origen { get; set; } = string.Empty;

    [MaxLength(150, ErrorMessage = "El destino no puede exceder 150 caracteres.")]
    public string Destino { get; set; } = "Tajamar";

    [Required(ErrorMessage = "La fecha y hora de salida es obligatoria.")]
    public DateTime FechaHoraSalida { get; set; }

    [Range(1, 9, ErrorMessage = "Las plazas deben ser entre 1 y 9.")]
    public int PlazasTotales { get; set; }

    [Required(ErrorMessage = "El tipo de trayecto es obligatorio ('Recurrente' o 'Puntual').")]
    public string TipoTrayecto { get; set; } = "Recurrente"; // Recurrente / Puntual

    [Required(ErrorMessage = "La modalidad es obligatoria ('SoloIda', 'SoloVuelta', 'IdaYVuelta').")]
    public string Modalidad { get; set; } = "IdaYVuelta";

    public string? DiasSemana { get; set; } // Ej: "L,M,X,J,V"
    public string? Turno { get; set; }      // "Manana" o "Tarde"

    public int? EventoId { get; set; }     // Opcional si es puntual de un evento
}
