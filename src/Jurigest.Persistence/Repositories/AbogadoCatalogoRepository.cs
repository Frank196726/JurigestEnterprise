using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class AbogadoCatalogoRepository
    : IAbogadoCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public AbogadoCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<AbogadoCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Abogados
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        CancellationToken cancellationToken)
    {
        var normalizado =
            nombre.Trim();

        return await _context.Abogados
            .AnyAsync(
                x => x.Nombre == normalizado,
                cancellationToken);
    }

    public async Task AddAsync(
        AbogadoCatalogo abogado,
        CancellationToken cancellationToken)
    {
        await _context.Abogados.AddAsync(
            abogado,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}