using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.API.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly TajamarAppContext _context;

    public UsuariosController(TajamarAppContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todos los usuarios de Tajamar registrados en el sistema (alumnos y profesores).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _context.UsuariosTajamar
            .AsNoTracking()
            .Include(u => u.Rol)
            .Select(u => new
            {
                u.IdUsuario,
                u.Nombre,
                u.Apellidos,
                u.Email,
                u.Estado,
                u.Imagen,
                Rol = u.Rol != null ? u.Rol.Role : "ALUMNO",
                TienePerfilApp = u.PerfilApp != null
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    /// <summary>
    /// Obtiene el detalle de un usuario por su ID de Tajamar.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUsuario(int id)
    {
        var usuario = await _context.UsuariosTajamar
            .AsNoTracking()
            .Include(u => u.Rol)
            .Include(u => u.PerfilApp)
            .Where(u => u.IdUsuario == id)
            .Select(u => new
            {
                u.IdUsuario,
                u.Nombre,
                u.Apellidos,
                u.Email,
                u.Estado,
                u.Imagen,
                Rol = u.Rol != null ? u.Rol.Role : "ALUMNO",
                PerfilApp = u.PerfilApp != null ? new
                {
                    u.PerfilApp.Id,
                    u.PerfilApp.Telefono,
                    u.PerfilApp.EsConductor,
                    u.PerfilApp.MarcaCoche,
                    u.PerfilApp.ModeloCoche,
                    u.PerfilApp.Matricula,
                    u.PerfilApp.PlazasCoche,
                    u.PerfilApp.Bio
                } : null
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            return NotFound(new { mensaje = $"Usuario con ID {id} no encontrado." });
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Lista los alumnos inscritos en un curso determinado (ej. 3597).
    /// </summary>
    [HttpGet("curso/{idCurso}")]
    public async Task<IActionResult> GetUsuariosPorCurso(int idCurso)
    {
        var alumnos = await _context.CursosUsuariosTajamar
            .AsNoTracking()
            .Where(cu => cu.IdCurso == idCurso)
            .Include(cu => cu.Usuario)
            .ThenInclude(u => u!.Rol)
            .Select(cu => new
            {
                cu.Usuario!.IdUsuario,
                cu.Usuario.Nombre,
                cu.Usuario.Apellidos,
                cu.Usuario.Email,
                cu.Usuario.Imagen,
                Rol = cu.Usuario.Rol != null ? cu.Usuario.Rol.Role : "ALUMNO"
            })
            .ToListAsync();

        return Ok(alumnos);
    }
}
