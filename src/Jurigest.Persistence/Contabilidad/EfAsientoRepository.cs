using Jurigest.Application.Contabilidad;
using Jurigest.Domain.Contabilidad.Entities;
using Jurigest.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Contabilidad;

public sealed class EfAsientoRepository : IAsientoRepository
{
    private readonly JurigestDbContext _context;

    public EfAsientoRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<AsientoContable>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Asientos
            .Include(a => a.Movimientos)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<AsientoContable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Asientos
            .Include(a => a.Movimientos)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task AddAsync(AsientoContable asiento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asiento);

        _context.Asientos.Add(asiento);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AsientoContable asiento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asiento);

        _context.Asientos.Update(asiento);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
