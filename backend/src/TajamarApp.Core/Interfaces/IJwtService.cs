using TajamarApp.Core.Entities;

namespace TajamarApp.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(UsuarioTajamar usuario, string rolNombre);
}
