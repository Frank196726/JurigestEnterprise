using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
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
        await context.ModelosEstampe.AddAsync(modelo, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
