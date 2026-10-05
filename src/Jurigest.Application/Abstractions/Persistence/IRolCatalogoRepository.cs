using Jurigest.Domain.Seguridad.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IRolCatalogoRepository
{
    Task<List<RolCatalogo>> GetAllAsync(CancellationToken ct);
    Task<RolCatalogo?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> TryAddAsync(RolCatalogo rol, CancellationToken ct);
}
