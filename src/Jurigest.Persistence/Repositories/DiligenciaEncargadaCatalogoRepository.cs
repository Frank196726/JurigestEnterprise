using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class DiligenciaEncargadaCatalogoRepository
    : IDiligenciaEncargadaCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public DiligenciaEncargadaCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<DiligenciaEncargadaCatalogo>> GetActivosAsync(
        CancellationToken cancellationToken)
    {
        return await _context.DiligenciasEncargadas
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

        return await _context.DiligenciasEncargadas
            .AnyAsync(
                x => x.Nombre == normalizado,
                cancellationToken);
    }

    public async Task AddAsync(
        DiligenciaEncargadaCatalogo diligencia,
        CancellationToken cancellationToken)
    {
        await _context.DiligenciasEncargadas.AddAsync(
            diligencia,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}