using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class TipoCausaCatalogoRepository
    : ITipoCausaCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public TipoCausaCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<TipoCausaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.TiposCausa
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken)
    {
        var normalizado = nombre.Trim();

        return await _context.TiposCausa
            .AnyAsync(
                x => x.Nombre == normalizado,
                cancellationToken);
    }

    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        CancellationToken cancellationToken)
    {
        var normalizado =
            codigo.Trim().ToUpperInvariant();

        return await _context.TiposCausa
            .AnyAsync(
                x => x.Codigo == normalizado,
                cancellationToken);
    }

    public async Task AddAsync(
        TipoCausaCatalogo tipo,
        CancellationToken cancellationToken)
    {
        await _context.TiposCausa.AddAsync(
            tipo,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<TipoCausaCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
{
    return await _context.TiposCausa
        .AsNoTracking()
        .FirstOrDefaultAsync(
            x => x.Id == id && x.Activo,
            cancellationToken);
}
}
