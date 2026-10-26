using Jurigest.Application.Judicial;
using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Persistence.Judicial;

public sealed class InMemoryCausaRepository : ICausaRepository
{
    private readonly Dictionary<Guid, Causa> _items = new();

    public Task<IReadOnlyCollection<Causa>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Causa>>(_items.Values.ToList());

    public Task<Causa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.TryGetValue(id, out var causa) ? causa : null);

    public Task AddAsync(Causa causa, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(causa);
        _items[causa.Id] = causa;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Causa causa, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(causa);
        _items[causa.Id] = causa;
        return Task.CompletedTask;
    }
}
