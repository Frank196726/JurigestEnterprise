using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface ITipoCausaCatalogoRepository
{
    Task<TipoCausaCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);


    Task<List<TipoCausaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken);

    Task<bool> ExisteCodigoAsync(
        string codigo,
        CancellationToken cancellationToken);

    Task AddAsync(
        TipoCausaCatalogo tipo,
        CancellationToken cancellationToken);
}