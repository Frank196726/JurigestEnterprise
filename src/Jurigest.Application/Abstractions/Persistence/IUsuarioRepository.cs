using Jurigest.Domain.Seguridad.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IUsuarioRepository
{
    Task AddAsync(
        Usuario usuario,
        CancellationToken cancellationToken);

    Task<Usuario?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Usuario?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<List<Usuario>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<bool> AnyAsync(
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Usuario usuario,
        CancellationToken cancellationToken);
}