using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Jurigest.Persistence.Repositories;

public sealed class RolCatalogoRepository(JurigestDbContext db) : IRolCatalogoRepository
{
    public Task<List<RolCatalogo>> GetAllAsync(CancellationToken ct) =>
        db.Set<RolCatalogo>().AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(ct);

    public Task<RolCatalogo?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Set<RolCatalogo>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> TryAddAsync(RolCatalogo rol, CancellationToken ct)
    {
        if (await db.Set<RolCatalogo>().AnyAsync(x => x.NombreNormalizado == rol.NombreNormalizado, ct)) return false;
        db.Set<RolCatalogo>().Add(rol);
        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            db.Entry(rol).State = EntityState.Detached;
            return false;
        }
    }
}
