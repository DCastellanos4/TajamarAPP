using System.ComponentModel.DataAnnotations;

namespace TajamarApp.Core.DTOs.Perfil;

public class PerfilAppDto
{
    [Phone(ErrorMessage = "El formato de teléfono no es válido.")]
    public string? Telefono { get; set; }

    public bool EsConductor { get; set; }

    public string? MarcaCoche { get; set; }
    public string? ModeloCoche { get; set; }
    public string? ColorCoche { get; set; }
    public string? Matricula { get; set; }

    [Range(0, 9, ErrorMessage = "Las plazas deben ser entre 0 y 9.")]
    public int PlazasCoche { get; set; }

    [MaxLength(500, ErrorMessage = "La biografía no puede superar los 500 caracteres.")]
    public string? Bio { get; set; }
}
