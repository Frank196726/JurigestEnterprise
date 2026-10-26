using Jurigest.Domain.Contabilidad.Entities;

namespace Jurigest.Application.Contabilidad;

public interface ICuentaRepository
{
    Task<IReadOnlyCollection<Cuenta>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Cuenta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Cuenta cuenta, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cuenta cuenta, CancellationToken cancellationToken = default);
}
