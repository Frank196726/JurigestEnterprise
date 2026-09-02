using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface ITribunalCatalogoRepository
{
    Task<List<TribunalCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken);

    Task AddAsync(
        TribunalCatalogo tribunal,
        CancellationToken cancellationToken);
}