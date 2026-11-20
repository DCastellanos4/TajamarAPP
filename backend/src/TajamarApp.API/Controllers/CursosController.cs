using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TajamarApp.Core.Entities;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly TajamarAppContext _context;

    public CursosController(TajamarAppContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todos los cursos de Tajamar registrados en la base de datos (con alumnos inscritos).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CursoTajamar>>> GetCursos()
    {
        var cursos = await _context.CursosTajamar
            .AsNoTracking()
            .ToListAsync();

        return Ok(cursos);
    }

    /// <summary>
    /// Obtiene el detalle de un curso por su ID (ej. 3597).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CursoTajamar>> GetCurso(int id)
    {
        var curso = await _context.CursosTajamar
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCurso == id);

        if (curso == null)
        {
            return NotFound(new { mensaje = $"Curso con ID {id} no encontrado." });
        }

        return Ok(curso);
    }
}
