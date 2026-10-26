using Jurigest.Application.Contabilidad;
using Jurigest.Domain.Contabilidad.Entities;

namespace Jurigest.Persistence.Contabilidad;

public sealed class InMemoryAsientoRepository : IAsientoRepository
{
    private readonly Dictionary<Guid, AsientoContable> _items = new();

    public Task<IReadOnlyCollection<AsientoContable>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<AsientoContable>>(_items.Values.ToList());

    public Task<AsientoContable?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.TryGetValue(id, out var asiento) ? asiento : null);

    public Task AddAsync(AsientoContable asiento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asiento);

        _items[asiento.Id] = asiento;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AsientoContable asiento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asiento);

        _items[asiento.Id] = asiento;
        return Task.CompletedTask;
    }
}
