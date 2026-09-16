using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IDiligenciaRealizadaCatalogoRepository
{
    Task<List<DiligenciaRealizadaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<List<DiligenciaRealizadaCatalogo>> GetActivosPorTipoAsync(
        int codigoTipoDiligencia,
        CancellationToken cancellationToken);

    Task<DiligenciaRealizadaCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        int codigoTipoDiligencia,
        CancellationToken cancellationToken);

    Task AddAsync(
        DiligenciaRealizadaCatalogo diligencia,
        CancellationToken cancellationToken);
}
