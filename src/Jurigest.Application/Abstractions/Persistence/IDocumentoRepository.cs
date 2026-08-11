using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IDocumentoRepository
{
    Task AddAsync(
        Documento documento,
        CancellationToken cancellationToken);

    Task<Documento?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<Documento>> GetByCausaIdAsync(
        Guid causaId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Documento documento,
        CancellationToken cancellationToken);
}