namespace Jurigest.Web.Security;

public interface IJurigestApiClient
{
    Task<T?> GetAsync<T>(
        string ruta,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PostAsync(
        string ruta,
        HttpContent contenido,
        CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PutAsync(
        string ruta,
        HttpContent contenido,
        CancellationToken cancellationToken = default);
}
