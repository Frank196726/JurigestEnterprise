using Jurigest.Domain.Contabilidad.Entities;

namespace Jurigest.Application.Contabilidad;

public interface IAsientoRepository
{
    Task<IReadOnlyCollection<AsientoContable>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AsientoContable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AsientoContable asiento, CancellationToken cancellationToken = default);
    Task UpdateAsync(AsientoContable asiento, CancellationToken cancellationToken = default);
}
