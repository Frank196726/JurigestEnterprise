using Jurigest.Domain.Seguridad.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IUsuarioRepository
{
    Task AddAsync(
        Usuario usuario,
        CancellationToken cancellationToken);

    Task<Usuario?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> AnyAsync(
        CancellationToken cancellationToken);
}