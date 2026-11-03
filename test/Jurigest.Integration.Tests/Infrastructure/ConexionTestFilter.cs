using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace Jurigest.Integration.Tests.Infrastructure;

/// <summary>
/// TestServer no tiene IP de origen: esta cabecera simula la dirección
/// desde la que llega la conexión TCP a la API.
/// </summary>
public sealed class ConexionTestFilter : IStartupFilter
{
    public const string Cabecera = "X-Test-Conexion-Ip";

    // Dirección que la API trata como Jurigest.Web en las pruebas.
    public const string IpWebConfiable = "10.0.0.10";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.Use(async (context, siguiente) =>
            {
                if (IPAddress.TryParse(context.Request.Headers[Cabecera], out var ip))
                    context.Connection.RemoteIpAddress = ip;

                await siguiente(context);
            });

            next(app);
        };
}

/// <summary>Agrega cabeceras fijas a todas las peticiones de un cliente de prueba.</summary>
public sealed class CabecerasFijasHandler(IReadOnlyDictionary<string, string> cabeceras)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        foreach (var (nombre, valor) in cabeceras)
            request.Headers.TryAddWithoutValidation(nombre, valor);

        return base.SendAsync(request, cancellationToken);
    }
}
