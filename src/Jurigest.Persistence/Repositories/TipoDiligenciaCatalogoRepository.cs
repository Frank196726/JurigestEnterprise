using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class TipoDiligenciaCatalogoRepository
    : ITipoDiligenciaCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public TipoDiligenciaCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<TipoDiligenciaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.TiposDiligencia
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<TipoDiligenciaCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.TiposDiligencia
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken)
    {
        var nombreNormalizado = nombre.Trim();

        return await _context.TiposDiligencia
            .AnyAsync(
                x => x.Nombre == nombreNormalizado,
                cancellationToken);
    }

    public async Task AddAsync(
        TipoDiligenciaCatalogo tipo,
        CancellationToken cancellationToken)
    {
        await _context.TiposDiligencia.AddAsync(
            tipo,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
}

    public async Task<int> ObtenerSiguienteCodigoPersonalizadoAsync(
        CancellationToken cancellationToken)
{
        var maximo = await _context.TiposDiligencia
            .Where(x =>
            x.CodigoSistema.HasValue &&
            x.CodigoSistema.Value >= 100)
            .MaxAsync(
            x => (int?)x.CodigoSistema,
            cancellationToken);

        return (maximo ?? 99) + 1;

    }
}
