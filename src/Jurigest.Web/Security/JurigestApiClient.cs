using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Jurigest.Web.Endpoints;

namespace Jurigest.Web.Security;

public sealed class JurigestApiClient
    : IJurigestApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISesionWebStore _sesionStore;

    // Se conserva el identificador obtenido al crear
    // el scope/circuito de Blazor.
    private readonly string? _identificadorSesionInicial;

    public JurigestApiClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ISesionWebStore sesionStore)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _sesionStore = sesionStore;

        httpContextAccessor.HttpContext?
            .Request.Cookies.TryGetValue(
                SeguridadWebEndpoints.CookieName,
                out _identificadorSesionInicial);
    }

    public async Task<T?> GetAsync<T>(
        string ruta,
        CancellationToken cancellationToken = default)
    {
        using var response =
            await SendAsync(
                HttpMethod.Get,
                ruta,
                contenido: null,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(
            cancellationToken: cancellationToken);
    }

    public Task<HttpResponseMessage> PostAsync(
        string ruta,
        HttpContent contenido,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            HttpMethod.Post,
            ruta,
            contenido,
            cancellationToken);
    }

    public Task<HttpResponseMessage> PutAsync(
        string ruta,
        HttpContent contenido,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(
            HttpMethod.Put,
            ruta,
            contenido,
            cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod metodo,
        string ruta,
        HttpContent? contenido,
        CancellationToken cancellationToken)
    {
        var sesion = ObtenerSesion();

        var client =
            _httpClientFactory.CreateClient(
                "JurigestApi");

        using var request =
            new HttpRequestMessage(
                metodo,
                ruta);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                sesion.AccessToken);

        if (contenido is not null)
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
                "El acceso al API expiró o no es válido.");
        }

        return response;
    }

    private SesionWeb ObtenerSesion()
    {
        var identificador =
            ObtenerIdentificadorSesion();

        if (string.IsNullOrWhiteSpace(
            identificador))
        {
            throw new UnauthorizedAccessException(
                "No existe una sesión web.");
        }

        var sesion =
            _sesionStore.Obtener(
                identificador);

        if (sesion is null)
        {
            throw new UnauthorizedAccessException(
                "La sesión web expiró.");
        }

        if (sesion.RefreshTokenExpiresAtUtc <=
            DateTime.UtcNow)
        {
            _sesionStore.Eliminar(
                identificador);

            throw new UnauthorizedAccessException(
                "La sesión web expiró.");
        }

        if (string.IsNullOrWhiteSpace(
            sesion.AccessToken))
        {
            throw new UnauthorizedAccessException(
                "La sesión no contiene un token de acceso.");
        }

        return sesion;
    }

    private string? ObtenerIdentificadorSesion()
    {
        // Primero usamos el identificador capturado
        // al crear el circuito.
        if (!string.IsNullOrWhiteSpace(
            _identificadorSesionInicial))
        {
            return _identificadorSesionInicial;
        }

        // Como recuperación, intentamos obtener nuevamente
        // la cookie si todavía existe un HttpContext.
        if (_httpContextAccessor.HttpContext?
            .Request.Cookies.TryGetValue(
                SeguridadWebEndpoints.CookieName,
                out var identificador) == true)
        {
            return identificador;
        }

        return null;
    }
}