namespace TajamarApp.Core.Entities;

public class EventoAsistente
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public int UsuarioTajamarId { get; set; }
    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public EventoApp Evento { get; set; } = null!;
    public UsuarioTajamar Usuario { get; set; } = null!;
}
