namespace TajamarApp.Core.Entities;

public class ColaEspera
{
    public int Id { get; set; }
    public int TrayectoId { get; set; }
    public int UsuarioTajamarId { get; set; }
    public int Posicion { get; set; } // 1, 2, 3...
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "EnEspera"; // EnEspera / Promocionado / Cancelado

    // Relaciones
    public Trayecto Trayecto { get; set; } = null!;
    public UsuarioTajamar Usuario { get; set; } = null!;
}
