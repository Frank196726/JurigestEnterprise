using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IResolucionRepository
{
    Task AddAsync(
        Resolucion resolucion,
        CancellationToken cancellationToken);

    Task<Resolucion?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<Resolucion>> GetByCausaIdAsync(
        Guid causaId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Resolucion resolucion,
        CancellationToken cancellationToken);
}