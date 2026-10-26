using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Judicial;

public interface ICausaRepository
{
    Task<IReadOnlyCollection<Causa>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Causa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Causa causa, CancellationToken cancellationToken = default);
    Task UpdateAsync(Causa causa, CancellationToken cancellationToken = default);
}
