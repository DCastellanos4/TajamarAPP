namespace TajamarApp.Core.Entities;

public class ReservaTrayecto
{
    public int Id { get; set; }
    public int TrayectoId { get; set; }
    public int PasajeroUsuarioId { get; set; }
    public DateTime FechaReserva { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Confirmada"; // Confirmada / Cancelada

    // Relaciones
    public Trayecto Trayecto { get; set; } = null!;
    public UsuarioTajamar Pasajero { get; set; } = null!;
}
