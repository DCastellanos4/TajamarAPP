using System.ComponentModel.DataAnnotations;

namespace TajamarApp.Core.DTOs.Trayectos;

public class FiltroTrayectosDto
{
    public string? Origen { get; set; }
    public string? Destino { get; set; }
    public DateTime? Fecha { get; set; }
    public string? TipoTrayecto { get; set; } // Recurrente / Puntual
    public string? Modalidad { get; set; }    // SoloIda / SoloVuelta / IdaYVuelta
    public string? Turno { get; set; }        // Manana / Tarde
    public int? EventoId { get; set; }
    public bool? SoloConPlazas { get; set; } = false;

    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor o igual a 1.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int TamanoPagina { get; set; } = 10;
}
