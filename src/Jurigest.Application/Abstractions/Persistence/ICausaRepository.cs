using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface ICausaRepository
{
    Task AddDemandadoAsync(Demandado demandado, CancellationToken cancellationToken);
    Task AddAvalSolidarioAsync(AvalSolidario aval, CancellationToken cancellationToken);
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

    // Un mismo ROL puede existir en varios tribunales.
    Task<List<Causa>> GetAllByRitAsync(
        string rit,
        CancellationToken cancellationToken);

    // Causas con el mismo número y año de ROL, sea cual sea la letra (C-2580-26, E-2580-26…).
    Task<List<Causa>> GetAllByNumeroRolAsync(
        string rit,
        CancellationToken cancellationToken);
}
