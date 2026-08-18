using System.Security.Claims;
using Jurigest.Web.Endpoints;
using Microsoft.AspNetCore.Components.Authorization;

namespace Jurigest.Web.Security;

public sealed class SesionAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private readonly AuthenticationState _estado;

    public SesionAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ISesionWebStore sesionStore)
    {
        var usuario =
            CrearUsuario(
                httpContextAccessor.HttpContext,
                sesionStore);

        _estado = new AuthenticationState(usuario);
    }

    public override Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        return Task.FromResult(_estado);
    }

    private static ClaimsPrincipal CrearUsuario(
        HttpContext? context,
        ISesionWebStore sesionStore)
    {
        if (context is null ||
            !context.Request.Cookies.TryGetValue(
                SeguridadWebEndpoints.CookieName,
                out var identificador))
        {
            return CrearAnonimo();
        }

        var sesion =
            sesionStore.Obtener(identificador);

        if (sesion is null)
            return CrearAnonimo();

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
                sesion.Rol)
        };

        var identidad = new ClaimsIdentity(
            claims,
            "JurigestSession");

        return new ClaimsPrincipal(identidad);
    }

    private static ClaimsPrincipal CrearAnonimo()
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity());
    }
}