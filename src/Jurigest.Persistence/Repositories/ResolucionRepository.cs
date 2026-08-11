using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class ResolucionRepository : IResolucionRepository
{
    private readonly JurigestDbContext _context;

    public ResolucionRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Resolucion resolucion,
        CancellationToken cancellationToken)
    {
        await _context.Resoluciones.AddAsync(
            resolucion,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Resolucion?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Resoluciones
            .AsNoTracking()
            .FirstOrDefaultAsync(
                resolucion => resolucion.Id == id,
                cancellationToken);
    }

    public async Task<List<Resolucion>> GetByCausaIdAsync(
        Guid causaId,
        CancellationToken cancellationToken)
    {
        return await _context.Resoluciones
            .AsNoTracking()
            .Where(resolucion => resolucion.CausaId == causaId)
            .OrderByDescending(resolucion => resolucion.Fecha)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Resolucion resolucion,
        CancellationToken cancellationToken)
    {
        _context.Resoluciones.Remove(resolucion);
        await _context.SaveChangesAsync(cancellationToken);
    }
}