using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TajamarApp.Core.DTOs.Perfil;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _perfilService;

    public PerfilController(IPerfilService perfilService)
    {
        _perfilService = perfilService;
    }

    /// <summary>
    /// Obtiene el perfil completo del alumno autenticado (datos institucionales + perfil extendido de conductor/vehículo).
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(PerfilMeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMiPerfil()
    {
        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido o sin identificador de usuario." });
        }

        var perfil = await _perfilService.GetMiPerfilAsync(idUsuario.Value);
        if (perfil == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        return Ok(perfil);
    }

    /// <summary>
    /// Guarda o actualiza el perfil extendido en la app (Onboarding de conductor, coche, plazas, teléfono).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PerfilAppDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GuardarPerfil([FromBody] PerfilAppDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var idUsuario = GetCurrentUserId();
        if (idUsuario == null)
        {
            return Unauthorized(new { mensaje = "Token inválido o sin identificador de usuario." });
        }

        var perfilActualizado = await _perfilService.GuardarPerfilAsync(idUsuario.Value, dto);
        return Ok(new
        {
            mensaje = "Perfil guardado correctamente.",
            perfil = perfilActualizado
        });
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
