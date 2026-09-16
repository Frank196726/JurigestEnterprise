using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class DiligenciaRealizadaCatalogoRepository
    : IDiligenciaRealizadaCatalogoRepository
{
    private readonly JurigestDbContext _context;

    public DiligenciaRealizadaCatalogoRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task<List<DiligenciaRealizadaCatalogo>>
        GetActivosAsync(
            CancellationToken cancellationToken)
    {
        return await _context.DiligenciasRealizadas
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.CodigoTipoDiligencia)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DiligenciaRealizadaCatalogo>>
        GetActivosPorTipoAsync(
            int codigoTipoDiligencia,
            CancellationToken cancellationToken)
    {
        return await _context.DiligenciasRealizadas
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                x.CodigoTipoDiligencia ==
                    codigoTipoDiligencia)
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<DiligenciaRealizadaCatalogo?>
        GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await _context.DiligenciasRealizadas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        int codigoTipoDiligencia,
        CancellationToken cancellationToken)
    {
        var normalizado =
            nombre.Trim();

        return await _context.DiligenciasRealizadas
            .AnyAsync(
                x =>
                    x.Nombre == normalizado &&
                    x.CodigoTipoDiligencia ==
                        codigoTipoDiligencia,
                cancellationToken);
    }

    public async Task AddAsync(
        DiligenciaRealizadaCatalogo diligencia,
        CancellationToken cancellationToken)
    {
        await _context.DiligenciasRealizadas.AddAsync(
            diligencia,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
