using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IDiligenciaEncargadaCatalogoRepository
{
    Task<List<DiligenciaEncargadaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken);

    Task AddAsync(
        DiligenciaEncargadaCatalogo diligencia,
        CancellationToken cancellationToken);
}