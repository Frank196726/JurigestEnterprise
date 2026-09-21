using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IReciboRepository
{
    Task<Recibo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Recibo?> GetByDiligenciaIdAsync(
        Guid diligenciaId,
        CancellationToken cancellationToken);

    Task<List<Recibo>> GetAllAsync(
        CancellationToken cancellationToken);

    Task AddAsync(
        Recibo recibo,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Recibo recibo,
        CancellationToken cancellationToken);
}
