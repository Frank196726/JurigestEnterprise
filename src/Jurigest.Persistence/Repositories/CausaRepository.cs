using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class CausaRepository : ICausaRepository
{
    private readonly JurigestDbContext _context;

    public CausaRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Causa causa,
        CancellationToken cancellationToken)
    {
        await _context.Causas.AddAsync(causa, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Causa?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Causas
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Causas
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Causa>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Causas
            .OrderBy(c => c.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Causa causa,
        CancellationToken cancellationToken)
    {
        _context.Causas.Update(causa);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteAsync(
    Causa causa,
    CancellationToken cancellationToken)
    {
    _context.Causas.Remove(causa);

    await _context.SaveChangesAsync(
        cancellationToken);
    }

    public async Task<Causa?> GetByRitAsync(
    string rit,
    CancellationToken cancellationToken)
    {
    return await _context.Causas
        .FirstOrDefaultAsync(
            c => c.Rit == rit,
            cancellationToken);
    }
}