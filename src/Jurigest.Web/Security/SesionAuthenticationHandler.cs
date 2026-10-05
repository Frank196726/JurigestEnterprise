using System.Security.Claims;
using System.Text.Encodings.Web;
using Jurigest.Web.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Jurigest.Web.Security;

public sealed class SesionAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName =
        "JurigestSession";

    private readonly ISesionWebStore _sesionStore;

    public SesionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISesionWebStore sesionStore)
        : base(
            options,
            logger,
            encoder)
    {
        _sesionStore = sesionStore;
    }

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(
                SeguridadWebEndpoints.CookieName,
                out var identificador))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var sesion =
            _sesionStore.Obtener(identificador);

        if (sesion is null || sesion.ExpiresAtUtc <= DateTime.UtcNow || sesion.RefreshTokenExpiresAtUtc <= DateTime.UtcNow)
        {
            return Task.FromResult(
                AuthenticateResult.Fail(
                    "La sesión no existe o expiró."));
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                sesion.UsuarioId.ToString()),

            new Claim(
                ClaimTypes.Name,
                sesion.Nombre),

            new Claim(
                ClaimTypes.Email,
                sesion.Email),

            new Claim(
                ClaimTypes.Role,
                sesion.Rol),

            new Claim("rol_asignado", sesion.RolAsignado),

            new Claim(
                "debe_cambiar_password",
                sesion.DebeCambiarPassword ? "true" : "false")
        };

        var identidad = new ClaimsIdentity(
            claims,
            SchemeName);

        var principal =
            new ClaimsPrincipal(identidad);

        var ticket =
            new AuthenticationTicket(
                principal,
                SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(
        AuthenticationProperties properties)
    {
        Response.Redirect("/login");
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Redirect("/acceso-denegado");
        return Task.CompletedTask;
    }
}
