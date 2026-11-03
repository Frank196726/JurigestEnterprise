using System.Net;

namespace Jurigest.API.Security;

/// <summary>
/// Toma la IP del usuario que informa Jurigest.Web en <see cref="Cabecera"/>,
/// solo si la conexión viene de una dirección configurada en
/// ReverseProxy:WebConfiable. Debe ir después de UseForwardedHeaders.
/// </summary>
public static class IpClienteWeb
{
    public const string Cabecera = "X-Jurigest-Cliente-Ip";

    public const string ClaveConfiguracion = "ReverseProxy:WebConfiable";

    public static IApplicationBuilder UseIpClienteWeb(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var confiables = LeerConfiables(configuration);

        return app.Use(async (context, next) =>
        {
            var valor = context.Request.Headers[Cabecera].ToString();
            context.Request.Headers.Remove(Cabecera);

            var origen = Normalizar(context.Connection.RemoteIpAddress);
            if (origen is not null && confiables.Contains(origen) &&
                IPAddress.TryParse(valor.Trim(), out var ipCliente))
            {
                context.Connection.RemoteIpAddress = Normalizar(ipCliente);
            }

            await next(context);
        });
    }

    private static HashSet<IPAddress> LeerConfiables(IConfiguration configuration)
    {
        var seccion = configuration.GetSection(ClaveConfiguracion);

        // Acepta "a,b" (variable de entorno) o un arreglo JSON.
        var valores = seccion.Value is { } texto
            ? texto.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
            : seccion.GetChildren().Select(x => x.Value ?? string.Empty);

        var confiables = new HashSet<IPAddress>();
        foreach (var valor in valores)
        {
            if (!IPAddress.TryParse(valor.Trim(), out var ip))
                throw new InvalidOperationException($"{ClaveConfiguracion} contiene una IP no válida: '{valor}'.");

            confiables.Add(Normalizar(ip)!);
        }

        return confiables;
    }

    private static IPAddress? Normalizar(IPAddress? ip) =>
        ip is { IsIPv4MappedToIPv6: true } ? ip.MapToIPv4() : ip;
}
