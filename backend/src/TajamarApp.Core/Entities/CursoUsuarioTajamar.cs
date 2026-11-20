namespace TajamarApp.Core.Entities;

public class CursoUsuarioTajamar
{
    public int IdCursosUsuarios { get; set; }
    public int? IdCurso { get; set; }
    public int? IdUsuario { get; set; }

    // Relaciones
    public CursoTajamar? Curso { get; set; }
    public UsuarioTajamar? Usuario { get; set; }
}
