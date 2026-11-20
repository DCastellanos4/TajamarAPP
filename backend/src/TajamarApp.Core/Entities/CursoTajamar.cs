namespace TajamarApp.Core.Entities;

public class CursoTajamar
{
    public int IdCurso { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool? Activo { get; set; }

    // Relaciones
    public ICollection<CursoUsuarioTajamar> Usuarios { get; set; } = new List<CursoUsuarioTajamar>();
}
