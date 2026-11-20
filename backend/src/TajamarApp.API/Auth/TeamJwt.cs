using System.Globalization;
using System.Security.Claims;

namespace TajamarApp.API.Auth;

public static class TeamJwt
{
    public static int UserId(ClaimsPrincipal user)
    {
        var idStr = user.FindFirstValue("idUsuarioTajamar")
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            throw new InvalidOperationException("Identificador de usuario no encontrado en el token.");
        }

        return id;
    }
}
