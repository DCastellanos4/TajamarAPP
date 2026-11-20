namespace TajamarApp.Core.Entities;

public class UsuarioTajamar
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public string? Imagen { get; set; }
    public string? Password { get; set; }
    public int IdRole { get; set; }

    // Relaciones
    public RolTajamar? Rol { get; set; }
    public ICollection<CursoUsuarioTajamar> Cursos { get; set; } = new List<CursoUsuarioTajamar>();
    public PerfilApp? PerfilApp { get; set; }
    public ICollection<ReservaTrayecto> Reservas { get; set; } = new List<ReservaTrayecto>();
    public ICollection<ColaEspera> ColasEspera { get; set; } = new List<ColaEspera>();
    public ICollection<EventoAsistente> AsistenciasEventos { get; set; } = new List<EventoAsistente>();
}
