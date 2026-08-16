using System.Security.Cryptography;
using System.Text;
using Jurigest.Application.Abstractions.Security;

namespace Jurigest.Persistence.Security;

public sealed class RefreshTokenService
    : IRefreshTokenService
{
    private const int TokenBytes = 64;

    public string GenerarToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(
            TokenBytes);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string CalcularHash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}