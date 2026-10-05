using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IModeloEstampeRepository
{
    Task<List<ModeloEstampe>> GetActivosAsync(CancellationToken cancellationToken);
    Task<List<ModeloEstampe>> GetTodosAsync(CancellationToken cancellationToken);
    Task<List<ModeloEstampe>> GetActivosAsync(TipoDiligencia tipoDiligencia,
        ResultadoDiligencia? resultado, CancellationToken cancellationToken);
    Task<ModeloEstampe?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExisteNombreAsync(string nombre, Guid? excluirId, CancellationToken cancellationToken);
    Task AddAsync(ModeloEstampe modelo, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
