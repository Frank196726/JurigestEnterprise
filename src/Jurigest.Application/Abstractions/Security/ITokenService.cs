using Jurigest.Domain.Seguridad.Entities;

namespace Jurigest.Application.Abstractions.Security;

public interface ITokenService
{
    TokenResult CrearToken(
    Usuario usuario,
    Guid sesionId);
}

public sealed record TokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc);