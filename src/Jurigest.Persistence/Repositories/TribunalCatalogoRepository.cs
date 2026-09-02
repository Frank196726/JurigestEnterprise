using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class TribunalCatalogoRepository
    : ITribunalCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public TribunalCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<TribunalCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Tribunales
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken)
    {
        var normalizado =
            nombre.Trim();

        return await _context.Tribunales
            .AnyAsync(
                x => x.Nombre == normalizado,
                cancellationToken);
    }

    public async Task AddAsync(
        TribunalCatalogo tribunal,
        CancellationToken cancellationToken)
    {
        await _context.Tribunales.AddAsync(
            tribunal,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}