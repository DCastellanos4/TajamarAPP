using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TajamarApp.Core.DTOs.Common;
using TajamarApp.Core.DTOs.Trayectos;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrayectosController : ControllerBase
{
    private readonly ITrayectoService _trayectoService;
    private readonly ICalculadoraGastosService _calculadoraGastosService;

    public TrayectosController(ITrayectoService trayectoService, ICalculadoraGastosService calculadoraGastosService)
    {
        _trayectoService = trayectoService;
        _calculadoraGastosService = calculadoraGastosService;
    }

    /// <summary>
    /// Calcula automáticamente el coste estimado de combustible por plaza, ahorro frente a taxi y CO2 ahorrado.
    /// </summary>
    [HttpGet("calcular-coste")]
    [ProducesResponseType(typeof(EstimacionCosteDto), StatusCodes.Status200OK)]
    public IActionResult CalcularCoste(
        [FromQuery] string? origen,
        [FromQuery] string? destino,
        [FromQuery] int plazas = 1)
    {
        var estimacion = _calculadoraGastosService.CalcularCoste(origen, destino, Math.Max(0, plazas));
        return Ok(estimacion);
    }

    /// <summary>
    /// Motor de búsqueda y filtrado de trayectos activos con paginación y filtros dinámicos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResultadoPaginadoDto<TrayectoResumenDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarTrayectos([FromQuery] FiltroTrayectosDto filtro)
    {
        var resultado = await _trayectoService.BuscarTrayectosAsync(filtro);
        return Ok(resultado);
    }

    /// <summary>
    /// Publica un nuevo trayecto (recurrente de clase o puntual de evento). Requiere perfil de conductor activo.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(TrayectoDetalleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CrearTrayecto([FromBody] CrearTrayectoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        try
        {
            var nuevoTrayecto = await _trayectoService.CrearTrayectoAsync(idUsuario.Value, dto);
            return CreatedAtAction(nameof(GetTrayectoPorId), new { id = nuevoTrayecto.Id }, nuevoTrayecto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el detalle completo de un trayecto por su ID (vehículo, conductor, plazas y evento).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrayectoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrayectoPorId(int id)
    {
        var trayecto = await _trayectoService.GetTrayectoPorIdAsync(id);
        if (trayecto == null)
        {
            return NotFound(new { mensaje = $"Trayecto con ID {id} no encontrado." });
        }

        return Ok(trayecto);
    }

    /// <summary>
    /// Lista todos los trayectos publicados por el conductor autenticado.
    /// </summary>
    [HttpGet("mis-viajes")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<TrayectoResumenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMisViajes()
    {
        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        var misViajes = await _trayectoService.GetMisTrayectosAsync(idUsuario.Value);
        return Ok(misViajes);
    }

    /// <summary>
    /// Modifica los datos de un trayecto propio.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(TrayectoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditarTrayecto(int id, [FromBody] EditarTrayectoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        try
        {
            var actualizado = await _trayectoService.EditarTrayectoAsync(idUsuario.Value, id, dto);
            if (actualizado == null)
            {
                return NotFound(new { mensaje = $"Trayecto con ID {id} no encontrado." });
            }

            return Ok(actualizado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Cancela un trayecto propio publicado por el conductor (cambia su estado a 'Cancelado').
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelarTrayecto(int id)
    {
        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        try
        {
            var resultado = await _trayectoService.CancelarTrayectoAsync(idUsuario.Value, id);
            if (!resultado)
            {
                return NotFound(new { mensaje = $"Trayecto con ID {id} no encontrado." });
            }

            return Ok(new { mensaje = "Trayecto cancelado correctamente." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Consulta los pasajeros confirmados y los alumnos en lista de espera de un trayecto propio.
    /// </summary>
    [HttpGet("{id:int}/pasajeros")]
    [Authorize]
    [ProducesResponseType(typeof(PasajerosTrayectoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPasajerosTrayecto(int id)
    {
        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        try
        {
            var resultado = await _trayectoService.GetPasajerosTrayectoAsync(idUsuario.Value, id);
            if (resultado == null)
            {
                return NotFound(new { mensaje = $"Trayecto con ID {id} no encontrado." });
            }

            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensaje = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst("idUsuarioTajamar")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
