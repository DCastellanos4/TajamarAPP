using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TajamarApp.Core.DTOs.Valoraciones;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.API.Controllers;

[ApiController]
[Tags("Valoraciones")]
public class ValoracionesController : ControllerBase
{
    private readonly IValoracionService _valoracionService;

    public ValoracionesController(IValoracionService valoracionService)
    {
        _valoracionService = valoracionService;
    }

    /// <summary>
    /// Emite una valoración y reseña sobre un trayecto en el que el usuario ha viajado con reserva confirmada.
    /// </summary>
    [HttpPost("api/valoraciones")]
    [Authorize]
    [ProducesResponseType(typeof(ResenaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CrearValoracion([FromBody] CrearValoracionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido o no autenticado." });
        }

        try
        {
            var resena = await _valoracionService.CrearValoracionAsync(idUsuario.Value, dto);
            return StatusCode(StatusCodes.Status201Created, resena);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
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
    /// Obtiene el resumen de calificaciones, desglose por estrellas y reseñas recibidas por un conductor.
    /// </summary>
    [HttpGet("api/conductores/{id:int}/resenas")]
    [ProducesResponseType(typeof(ResumenResenasConductorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResenasConductor(int id)
    {
        try
        {
            var resumen = await _valoracionService.GetResenasConductorAsync(id);
            return Ok(resumen);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
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
