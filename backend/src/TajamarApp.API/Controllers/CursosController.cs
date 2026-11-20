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
    /// Obtiene todos los cursos de Tajamar registrados en la base de datos.
    /// Si la tabla está vacía, inserta cursos de ejemplo iniciales.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CursoTajamar>>> GetCursos()
    {
        var cursos = await _context.CursosTajamar.ToListAsync();

        // Si aún no hay cursos en Azure SQL, sembramos datos iniciales de Tajamar
        if (!cursos.Any())
        {
            var seedCursos = new List<CursoTajamar>
            {
                new() { Nombre = "Desarrollo Web Full Stack (MCSD Mañana)", Activo = true, FechaInicio = DateTime.UtcNow },
                new() { Nombre = "Desarrollo Web Full Stack (MCSD Tarde)", Activo = true, FechaInicio = DateTime.UtcNow },
                new() { Nombre = "Cloud & DevOps Azure", Activo = true, FechaInicio = DateTime.UtcNow },
                new() { Nombre = "Ciberseguridad y Redes", Activo = true, FechaInicio = DateTime.UtcNow }
            };

            await _context.CursosTajamar.AddRangeAsync(seedCursos);
            await _context.SaveChangesAsync();

            cursos = await _context.CursosTajamar.ToListAsync();
        }

        return Ok(cursos);
    }

    /// <summary>
    /// Obtiene el detalle de un curso por su identificador.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CursoTajamar>> GetCurso(int id)
    {
        var curso = await _context.CursosTajamar.FindAsync(id);
        if (curso == null)
        {
            return NotFound(new { mensaje = $"Curso con ID {id} no encontrado." });
        }
        return Ok(curso);
    }

    /// <summary>
    /// Registra un nuevo curso en Tajamar.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CursoTajamar>> CreateCurso([FromBody] CursoTajamar curso)
    {
        _context.CursosTajamar.Add(curso);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurso), new { id = curso.IdCurso }, curso);
    }
}
