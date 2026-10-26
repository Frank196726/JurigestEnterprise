using Jurigest.Application.Judicial;
using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Persistence.Judicial;

public sealed class InMemoryEncargoRepository : IEncargoRepository
{
    private readonly Dictionary<Guid, Encargo> _items = new();

    public Task<IReadOnlyCollection<Encargo>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Encargo>>(_items.Values.ToList());

    public Task<Encargo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.TryGetValue(id, out var encargo) ? encargo : null);

    public Task AddAsync(Encargo encargo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(encargo);
        _items[encargo.Id] = encargo;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Encargo encargo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(encargo);
        _items[encargo.Id] = encargo;
        return Task.CompletedTask;
    }
}
