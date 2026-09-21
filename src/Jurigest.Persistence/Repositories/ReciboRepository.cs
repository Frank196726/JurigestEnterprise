using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class ReciboRepository : IReciboRepository
{
    private readonly JurigestDbContext _context;

    public ReciboRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<Recibo?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Recibos
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Recibo?> GetByDiligenciaIdAsync(
        Guid diligenciaId,
        CancellationToken cancellationToken)
    {
        return await _context.Recibos
            .FirstOrDefaultAsync(
                x => x.DiligenciaId == diligenciaId,
                cancellationToken);
    }

    public async Task<List<Recibo>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Recibos
            .AsNoTracking()
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Recibo recibo,
        CancellationToken cancellationToken)
    {
        await _context.Recibos.AddAsync(
            recibo,
            cancellationToken);
    }

    public async Task UpdateAsync(
        Recibo recibo,
        CancellationToken cancellationToken)
    {
        _context.Recibos.Update(recibo);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
