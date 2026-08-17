using Jurigest.Domain.Seguridad.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface ITokenRecuperacionPasswordRepository
{
    Task ReplaceActiveAsync(
        TokenRecuperacionPassword token,
        DateTime fechaUtc,
        CancellationToken cancellationToken);

    Task<TokenRecuperacionPassword?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task<bool> CompleteAsync(
        TokenRecuperacionPassword token,
        Usuario usuario,
        CancellationToken cancellationToken);
}