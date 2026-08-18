using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;

namespace Jurigest.Web.Security;

public sealed class MemorySesionWebStore : ISesionWebStore
{
    private readonly IMemoryCache _cache;

    public MemorySesionWebStore(
        IMemoryCache cache)
    {
        _cache = cache;
    }

    public string Crear(
        SesionWeb sesion)
    {
        var bytes =
            RandomNumberGenerator.GetBytes(32);

        var identificador =
            WebEncoders.Base64UrlEncode(bytes);

        Actualizar(
            identificador,
            sesion);

        return identificador;
    }

    public SesionWeb? Obtener(
        string identificador)
    {
        if (string.IsNullOrWhiteSpace(identificador))
            return null;

        return _cache.TryGetValue(
            identificador,
            out SesionWeb? sesion)
            ? sesion
            : null;
    }

    public void Actualizar(
        string identificador,
        SesionWeb sesion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            identificador);

        ArgumentNullException.ThrowIfNull(
            sesion);

        _cache.Set(
            identificador,
            sesion,
            sesion.RefreshTokenExpiresAtUtc);
    }

    public void Eliminar(
        string identificador)
    {
        if (!string.IsNullOrWhiteSpace(identificador))
            _cache.Remove(identificador);
    }
}