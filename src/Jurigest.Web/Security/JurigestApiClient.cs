using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jurigest.Web.Endpoints;

namespace Jurigest.Web.Security;

public sealed class JurigestApiClient
    : IJurigestApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISesionWebStore _sesionStore;
    private readonly string? _identificadorSesion;

    public JurigestApiClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ISesionWebStore sesionStore)
    {
        _httpClientFactory = httpClientFactory;
        _sesionStore = sesionStore;

        httpContextAccessor.HttpContext?
            .Request.Cookies.TryGetValue(
                SeguridadWebEndpoints.CookieName,
                out _identificadorSesion);
    }

    public async Task<T?> GetAsync<T>(
        string ruta,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
                _identificadorSesion))
        {
            throw new UnauthorizedAccessException(
                "No existe una sesión web.");
        }

        var sesion =
            _sesionStore.Obtener(
                _identificadorSesion);

        if (sesion is null)
        {
            throw new UnauthorizedAccessException(
                "La sesión web expiró.");
        }

        var client =
            _httpClientFactory.CreateClient(
                "JurigestApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                ruta);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                sesion.AccessToken);

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "El acceso al API expiró.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<T>(
                cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync(
        string ruta,
        HttpContent contenido,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
                _identificadorSesion))
        {
            throw new UnauthorizedAccessException(
                "No existe una sesión web.");
        }

        var sesion =
            _sesionStore.Obtener(
                _identificadorSesion);

        if (sesion is null)
        {
            throw new UnauthorizedAccessException(
                "La sesión web expiró.");
        }

        var client =
            _httpClientFactory.CreateClient(
                "JurigestApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                ruta);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                sesion.AccessToken);

        request.Content = contenido;

        var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            response.Dispose();

            throw new UnauthorizedAccessException(
                "El acceso al API expiró.");
        }

        return response;
    }

    public Task<HttpResponseMessage> PutAsync(
        string ruta,
        HttpContent contenido,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Put, ruta, contenido, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod metodo,
        string ruta,
        HttpContent contenido,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_identificadorSesion))
            throw new UnauthorizedAccessException("No existe una sesión web.");

        var sesion = _sesionStore.Obtener(_identificadorSesion);

        if (sesion is null)
            throw new UnauthorizedAccessException("La sesión web expiró.");

        var client = _httpClientFactory.CreateClient("JurigestApi");
        using var request = new HttpRequestMessage(metodo, ruta);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            sesion.AccessToken);
        request.Content = contenido;

        var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            throw new UnauthorizedAccessException("El acceso al API expiró.");
        }

        return response;
    }

}
