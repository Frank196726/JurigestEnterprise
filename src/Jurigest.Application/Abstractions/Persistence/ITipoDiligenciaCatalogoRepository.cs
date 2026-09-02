using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface ITipoDiligenciaCatalogoRepository
{
    Task<List<TipoDiligenciaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<TipoDiligenciaCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken);

    Task AddAsync(
        TipoDiligenciaCatalogo tipo,
        CancellationToken cancellationToken);

    Task<int> ObtenerSiguienteCodigoPersonalizadoAsync(
        CancellationToken cancellationToken);
}
