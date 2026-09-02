using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IAbogadoCatalogoRepository
{
    Task<List<AbogadoCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken);

    Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken);

    Task AddAsync(
        AbogadoCatalogo abogado,
        CancellationToken cancellationToken);
}