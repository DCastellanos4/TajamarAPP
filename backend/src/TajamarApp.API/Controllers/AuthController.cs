using Microsoft.AspNetCore.Mvc;
using TajamarApp.Core.DTOs.Auth;
using TajamarApp.Core.Interfaces;

namespace TajamarApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Inicia sesión con las credenciales oficiales de Tajamar (email y contraseña '12345').
    /// Devuelve el token JWT y el estado de completitud del perfil en la app.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas. Verifica tu email institucional y contraseña." });
        }

        return Ok(response);
    }
}
