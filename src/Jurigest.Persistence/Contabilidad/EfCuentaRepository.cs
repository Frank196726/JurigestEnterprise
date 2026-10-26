using Jurigest.Application.Contabilidad;
using Jurigest.Domain.Contabilidad.Entities;
using Jurigest.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Contabilidad;

public sealed class EfCuentaRepository : ICuentaRepository
{
    private readonly JurigestDbContext _context;

    public EfCuentaRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Cuenta>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Cuentas
            .Include(c => c.Movimientos)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<Cuenta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Cuentas
            .Include(c => c.Movimientos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cuenta);

        _context.Cuentas.Add(cuenta);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cuenta);

        _context.Cuentas.Update(cuenta);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
