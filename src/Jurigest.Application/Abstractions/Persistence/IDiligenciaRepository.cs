using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IDiligenciaRepository
{
    Task<List<Diligencia>> GetAllAsync(
        CancellationToken cancellationToken);

    Task AddAsync(
        Diligencia diligencia,
        CancellationToken cancellationToken);

    Task<Diligencia?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<Diligencia>> GetByCausaAsync(
        Guid causaId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Diligencia diligencia,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Diligencia diligencia,
        CancellationToken cancellationToken);

        Task<List<Diligencia>> GetByCausaIdAsync(
    Guid causaId,
    CancellationToken cancellationToken);

    Task<Diligencia?> GetUltimaByCausaAsync(
    Guid causaId,
    CancellationToken cancellationToken);
}
