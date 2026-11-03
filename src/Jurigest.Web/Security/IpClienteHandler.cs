using System.Net;

namespace Jurigest.Web.Security;

/// <summary>
/// Informa a la API la IP del usuario en cada llamada del cliente "JurigestApi".
/// La API solo la acepta si la Web figura en ReverseProxy:WebConfiable.
/// </summary>
public sealed class IpClienteHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    public const string Cabecera = "X-Jurigest-Cliente-Ip";

    // En un circuito Blazor el HttpContext puede faltar; IpClienteCircuito
    // guarda la IP al crear el circuito y la pasa por aquí.
    public static readonly HttpRequestOptionsKey<IPAddress?> IpCapturada =
        new("Jurigest.IpCliente");

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Remove(Cabecera);

        if (!request.Options.TryGetValue(IpCapturada, out var ip) || ip is null)
            ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;

        if (ip is not null)
        {
            if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
            request.Headers.TryAddWithoutValidation(Cabecera, ip.ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
