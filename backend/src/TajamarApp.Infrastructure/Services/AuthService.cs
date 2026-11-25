using Microsoft.EntityFrameworkCore;
using TajamarApp.Core.DTOs.Auth;
using TajamarApp.Core.Interfaces;
using TajamarApp.Infrastructure.Data;

namespace TajamarApp.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly TajamarAppContext _context;
    private readonly IJwtService _jwtService;

    // Hash oficial de Paco para la contraseña por defecto "12345"
    private const string PACO_DEFAULT_HASH = "OFi9W3uf5kGsaxyTqF1ZFQ==";

    public AuthService(TajamarAppContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        // 1. Buscar usuario por email en USUARIOSTAJAMAR con su Rol y su PerfilApp
        var usuario = await _context.UsuariosTajamar
            .Include(u => u.Rol)
            .Include(u => u.PerfilApp)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);

        if (usuario == null)
        {
            return null; // Usuario no existe
        }

        // 2. Validar contraseña
        // Soporta la clave por defecto '12345' (hash OFi9W3uf5kGsaxyTqF1ZFQ==) o coincidencia directa
        bool passwordValida = (request.Password == "12345" && usuario.Password == PACO_DEFAULT_HASH)
                           || (request.Password == usuario.Password)
                           || (request.Password == PACO_DEFAULT_HASH);

        if (!passwordValida)
        {
            return null; // Contraseña incorrecta
        }

        // 3. Obtener nombre de rol
        var rolNombre = usuario.Rol?.Role ?? "ALUMNO";

        // 4. Generar Token JWT
        var token = _jwtService.GenerateToken(usuario, rolNombre);

        // 5. Determinar si ya completó el perfil de la app
        bool tienePerfilCompletado = usuario.PerfilApp != null;

        return new LoginResponseDto
        {
            Token = token,
            PerfilCompletado = tienePerfilCompletado,
            Usuario = new UsuarioBasicoDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Imagen = usuario.Imagen,
                Rol = rolNombre
            }
        };
    }
}
