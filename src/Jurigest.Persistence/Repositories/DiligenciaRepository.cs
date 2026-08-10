using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class DiligenciaRepository : IDiligenciaRepository
{
    private readonly JurigestDbContext _context;

    public DiligenciaRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Diligencia diligencia,
        CancellationToken cancellationToken)
    {
        await _context.Diligencias.AddAsync(
            diligencia,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Diligencia?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Diligencias
            .FirstOrDefaultAsync(
                d => d.Id == id,
                cancellationToken);
    }

    public async Task<List<Diligencia>> GetByCausaAsync(
        Guid causaId,
        CancellationToken cancellationToken)
    {
        return await _context.Diligencias
            .Where(d => d.CausaId == causaId)
            .OrderBy(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Diligencia diligencia,
        CancellationToken cancellationToken)
    {
        _context.Diligencias.Update(diligencia);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        Diligencia diligencia,
        CancellationToken cancellationToken)
    {
        _context.Diligencias.Remove(diligencia);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
    public async Task<List<Diligencia>> GetByCausaIdAsync(
        Guid causaId,
        CancellationToken cancellationToken)
{
    return await _context.Diligencias
        .Where(d => d.CausaId == causaId)
        .OrderBy(d => d.FechaCreacion)
        .ToListAsync(cancellationToken);
}

}