namespace Jurigest.Web.Security;

public interface IJurigestApiClient
{
    Task<T?> GetAsync<T>(
        string ruta,
        CancellationToken cancellationToken = default);
}