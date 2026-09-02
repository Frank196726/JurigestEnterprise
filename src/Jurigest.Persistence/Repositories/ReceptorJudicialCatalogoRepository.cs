using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class ReceptorJudicialCatalogoRepository
    : IReceptorJudicialCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public ReceptorJudicialCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReceptorJudicialCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.ReceptoresJudiciales
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReceptorJudicialCatalogo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.ReceptoresJudiciales
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken)
    {
        var nombreNormalizado = nombre.Trim();

        return await _context.ReceptoresJudiciales
            .AnyAsync(
                x => x.Nombre == nombreNormalizado,
                cancellationToken);
    }

    public async Task AddAsync(
        ReceptorJudicialCatalogo receptor,
        CancellationToken cancellationToken)
    {
        await _context.ReceptoresJudiciales.AddAsync(
            receptor,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
