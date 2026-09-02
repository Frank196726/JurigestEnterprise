using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class ComunaCatalogoRepository
    : IComunaCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public ComunaCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComunaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Comunas
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<ComunaCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Comunas
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken)
    {
        var nombreNormalizado = nombre.Trim();

        return await _context.Comunas
            .AnyAsync(
                x => x.Nombre == nombreNormalizado,
                cancellationToken);
    }

    public async Task AddAsync(
        ComunaCatalogo comuna,
        CancellationToken cancellationToken)
    {
        await _context.Comunas.AddAsync(
            comuna,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
