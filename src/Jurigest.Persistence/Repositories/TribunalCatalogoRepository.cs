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
        var tribunales = await _context.Tribunales
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return tribunales.GroupBy(x => x.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(grupo => grupo.OrderBy(x => x.Id).First()).ToList();
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken)
    {
        var nombres = await _context.Tribunales.AsNoTracking()
            .Select(x => x.Nombre).ToListAsync(cancellationToken);
        return nombres.Any(x => Jurigest.Domain.Judicial.IdentificacionCausa.MismoTribunal(x, nombre));
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