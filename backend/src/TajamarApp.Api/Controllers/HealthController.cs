using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly TajamarAppContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(TajamarAppContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Comprueba el estado de la API y la conectividad con Azure SQL Database.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var totalCursos = await _context.CursosTajamar.CountAsync();
            var totalUsuarios = await _context.UsuariosTajamar.SumAsync(usuario => usuario.IdUsuario > 0 ? 1 : 0);

            return Ok(new
            {
                status = "Online",
                database = "Azure SQL (AZURETAJAMAR)",
                canConnect,
                totalCursosRegistrados = totalCursos,
                totalUsuariosRegistrados = totalUsuarios,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "Error",
                database = "Fallo de conexión con Azure SQL",
                error = ex.Message
            });
        }
    }
}
