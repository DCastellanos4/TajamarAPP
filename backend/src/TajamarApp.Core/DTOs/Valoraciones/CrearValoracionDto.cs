using System.ComponentModel.DataAnnotations;

namespace TajamarApp.Core.DTOs.Valoraciones;

public class CrearValoracionDto
{
    [Required(ErrorMessage = "El ID del trayecto es obligatorio.")]
    public int TrayectoId { get; set; }

    [Range(1, 5, ErrorMessage = "La puntuación debe ser un número entero entre 1 y 5.")]
    public int Puntuacion { get; set; }

    [MaxLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
    public string? Comentario { get; set; }

    [MaxLength(250, ErrorMessage = "Los tags no pueden superar los 250 caracteres.")]
    public string? Tags { get; set; }
}
