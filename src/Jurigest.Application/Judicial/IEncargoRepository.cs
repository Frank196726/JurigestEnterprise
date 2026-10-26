using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Judicial;

public interface IEncargoRepository
{
    Task<IReadOnlyCollection<Encargo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Encargo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Encargo encargo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Encargo encargo, CancellationToken cancellationToken = default);
}
