using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

sealed class TeamJwt(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
    UrlEncoder encoder, IConfiguration config) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        const string prefix = "Bearer ";
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());

        var key = config["Jwt:SigningKey"];
        var issuer = config["Jwt:Issuer"];
        var audience = config["Jwt:Audience"];
        if (key is null || Encoding.UTF8.GetByteCount(key) < 32 ||
            string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            return Task.FromResult(AuthenticateResult.Fail("JWT sin configurar."));

        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(header[prefix.Length..].Trim(), new TokenValidationParameters
            {
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                NameClaimType = "nombre",
                RoleClaimType = "role",
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);
            if (!int.TryParse(principal.FindFirstValue("idUsuarioTajamar"), NumberStyles.None,
                    CultureInfo.InvariantCulture, out var id) || id <= 0)
                return Task.FromResult(AuthenticateResult.Fail("Identificador de usuario inválido."));

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return Task.FromResult(AuthenticateResult.Fail("JWT inválido."));
        }
    }

    internal static int UserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue("idUsuarioTajamar")!, CultureInfo.InvariantCulture);
}
