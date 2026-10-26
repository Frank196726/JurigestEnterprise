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

    Task<bool> ExistsByRutAsync(
        string rut,
        CancellationToken cancellationToken);

    Task<bool> AnyAsync(
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Usuario usuario,
        CancellationToken cancellationToken);

    /// <summary>
    /// Crea el usuario junto con sus receptores asociados en una sola operación.
    /// </summary>
    Task AddAsync(
        Usuario usuario,
        IReadOnlyCollection<Guid> receptorIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Actualiza el usuario y reemplaza sus receptores asociados en una sola operación.
    /// </summary>
    Task UpdateAsync(
        Usuario usuario,
        IReadOnlyCollection<Guid> receptorIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Receptores asociados a los usuarios indicados, o a todos si es null.
    /// </summary>
    Task<List<UsuarioReceptorAsignado>> GetReceptoresAsignadosAsync(
        IReadOnlyCollection<Guid>? usuarioIds,
        CancellationToken cancellationToken);
}

public sealed record UsuarioReceptorAsignado(
    Guid UsuarioId,
    Guid ReceptorId,
    string Nombre);
