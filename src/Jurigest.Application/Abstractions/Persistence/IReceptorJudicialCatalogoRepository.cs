using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IReceptorJudicialCatalogoRepository
{
    Task<List<ReceptorJudicialCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<ReceptorJudicialCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken);

    Task AddAsync(
        ReceptorJudicialCatalogo receptor,
        CancellationToken cancellationToken);
}
