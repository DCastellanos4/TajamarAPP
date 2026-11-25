namespace TajamarApp.Core.DTOs.Notificaciones;

public class NotificacionDto
{
    public string Tipo { get; set; } = string.Empty; // "NUEVA_RESERVA", "PROMOCION_COLA", "TRAYECTO_CANCELADO"
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object?> Datos { get; set; } = new();
}
