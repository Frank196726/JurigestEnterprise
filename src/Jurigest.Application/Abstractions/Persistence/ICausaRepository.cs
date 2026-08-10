using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface ICausaRepository
{
    Task AddAsync(
        Causa causa,
        CancellationToken cancellationToken);

    Task<Causa?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<Causa>> GetAllAsync(
        CancellationToken cancellationToken);

        Task UpdateAsync(
    Causa causa,
    CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken);

        Task SaveChangesAsync(
    CancellationToken cancellationToken);

    Task DeleteAsync(
        Causa causa,
        CancellationToken cancellationToken);

    Task<Causa?> GetByRitAsync(
        string rit,
        CancellationToken cancellationToken);
}