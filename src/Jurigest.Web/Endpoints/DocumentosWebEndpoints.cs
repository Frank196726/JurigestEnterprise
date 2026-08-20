using System.Net;
using System.Net.Http.Headers;
using Jurigest.Web.Security;

namespace Jurigest.Web.Endpoints;

public static class DocumentosWebEndpoints
{
    public static IEndpointRouteBuilder MapDocumentosWebEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/documentos/{id:guid}/archivo",
                DescargarAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> DescargarAsync(
        Guid id,
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        ISesionWebStore sesionStore,
        CancellationToken cancellationToken)
    {
        if (!context.Request.Cookies.TryGetValue(
                SeguridadWebEndpoints.CookieName,
                out var identificadorSesion) ||
            string.IsNullOrWhiteSpace(identificadorSesion))
        {
            return Results.Unauthorized();
        }

        var sesion =
            sesionStore.Obtener(identificadorSesion);

        if (sesion is null)
            return Results.Unauthorized();

        var client =
            httpClientFactory.CreateClient("JurigestApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/Documentos/{id}/archivo");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                sesion.AccessToken);

        using var response =
            await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return Results.Unauthorized();

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return Results.Forbid();

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Results.NotFound();

        if (!response.IsSuccessStatusCode)
        {
            return Results.StatusCode(
                (int)response.StatusCode);
        }

        var contenido =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        var contentType =
            response.Content.Headers.ContentType?
                .MediaType ??
            "application/octet-stream";

        var nombreArchivo =
            response.Content.Headers.ContentDisposition?
                .FileNameStar ??
            response.Content.Headers.ContentDisposition?
                .FileName ??
            $"documento-{id}";

        nombreArchivo = nombreArchivo.Trim('"');

        return Results.File(
            contenido,
            contentType,
            nombreArchivo);
    }
}