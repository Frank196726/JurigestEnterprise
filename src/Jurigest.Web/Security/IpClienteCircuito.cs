using System.Net;

namespace Jurigest.Web.Security;

/// <summary>
/// IP del usuario capturada al crear el scope (petición o circuito Blazor),
/// cuando el HttpContext todavía está disponible.
/// </summary>
public sealed class IpClienteCircuito(IHttpContextAccessor httpContextAccessor)
{
    public IPAddress? Ip { get; } =
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

    /// <summary>Marca la petición para que IpClienteHandler informe esta IP.</summary>
    public void Aplicar(HttpRequestMessage request) =>
        request.Options.Set(IpClienteHandler.IpCapturada, Ip);
}
