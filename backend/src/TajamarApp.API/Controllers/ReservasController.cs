using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TajamarApp.Core.DTOs.Reservas;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    /// <summary>
    /// Consulta todos los viajes donde el alumno autenticado tiene plaza reservada o está en lista de espera.
    /// </summary>
    [HttpGet("mis-reservas")]
    [ProducesResponseType(typeof(IEnumerable<MiReservaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMisReservas()
    {
        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        var resultado = await _reservaService.GetMisReservasAsync(idUsuario.Value);
        return Ok(resultado);
    }

    /// <summary>
    /// Permite al alumno salir voluntariamente de una lista de espera de un trayecto.
    /// </summary>
    [HttpDelete("cola/{idCola:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelarCola(int idCola)
    {
        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido." });
        }

        var cancelado = await _reservaService.CancelarColaAsync(idUsuario.Value, idCola);
        if (!cancelado)
        {
            return NotFound(new { mensaje = "Inscripción en lista de espera no encontrada o ya cancelada." });
        }

        return Ok(new { mensaje = "Has salido de la lista de espera correctamente." });
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst("idUsuarioTajamar")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return int.TryParse(claim, out var id) ? id : null;
    }
}
