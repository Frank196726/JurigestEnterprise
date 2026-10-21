using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class ModeloEstampeRepository(JurigestDbContext context) : IModeloEstampeRepository
{
    public Task<List<ModeloEstampe>> GetActivosAsync(CancellationToken cancellationToken) =>
        context.ModelosEstampe.AsNoTracking().Where(x => x.Activo)
            .OrderBy(x => x.TipoDiligencia).ThenBy(x => x.Nombre).ToListAsync(cancellationToken);

    public Task<List<ModeloEstampe>> GetTodosAsync(CancellationToken cancellationToken) =>
        context.ModelosEstampe.AsNoTracking().OrderBy(x => x.TipoDiligencia)
            .ThenBy(x => x.Nombre).ToListAsync(cancellationToken);

    public Task<List<ModeloEstampe>> GetActivosAsync(TipoDiligencia tipoDiligencia,
        ResultadoDiligencia? resultado, CancellationToken cancellationToken) =>
        context.ModelosEstampe.AsNoTracking().Where(x => x.Activo &&
            x.TipoDiligencia == tipoDiligencia && (x.Resultado == null || x.Resultado == resultado))
            .OrderBy(x => x.Resultado.HasValue).ThenBy(x => x.Nombre).ToListAsync(cancellationToken);

    public Task<ModeloEstampe?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.ModelosEstampe.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId, CancellationToken cancellationToken) =>
        context.ModelosEstampe.AnyAsync(x => x.Nombre == nombre.Trim() &&
            (!excluirId.HasValue || x.Id != excluirId.Value), cancellationToken);

    public async Task AddAsync(ModeloEstampe modelo, CancellationToken cancellationToken)
    {
        await VincularGestionAsync(modelo, cancellationToken);
        await context.ModelosEstampe.AddAsync(modelo, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var modelos = context.ChangeTracker.Entries<ModeloEstampe>()
            .Where(x => x.State == EntityState.Modified).Select(x => x.Entity).ToList();
        foreach (var modelo in modelos) await VincularGestionAsync(modelo, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task VincularGestionAsync(ModeloEstampe modelo, CancellationToken cancellationToken)
    {
        if (modelo.DiligenciaRealizadaId.HasValue) return;
        var nombreGestion = (int)modelo.TipoDiligencia == 112 ? "Búsqueda" : modelo.Nombre;
        var gestion = await context.DiligenciasRealizadas.FirstOrDefaultAsync(x =>
            x.Nombre == nombreGestion && x.CodigoTipoDiligencia == (int)modelo.TipoDiligencia,
            cancellationToken);
        if (gestion is null)
        {
            gestion = new DiligenciaRealizadaCatalogo(Guid.NewGuid(), nombreGestion, (int)modelo.TipoDiligencia);
            context.DiligenciasRealizadas.Add(gestion);
        }
        else gestion.Activar();
        modelo.Actualizar(modelo.Nombre, modelo.TipoDiligencia, modelo.Contenido, modelo.Resultado, gestion.Id);
    }
}
