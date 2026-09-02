using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IComunaCatalogoRepository
{
    Task<List<ComunaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<ComunaCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken);

    Task AddAsync(
        ComunaCatalogo comuna,
        CancellationToken cancellationToken);
}
