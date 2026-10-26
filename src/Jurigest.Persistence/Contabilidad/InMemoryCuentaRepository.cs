using Jurigest.Application.Contabilidad;
using Jurigest.Domain.Contabilidad.Entities;

namespace Jurigest.Persistence.Contabilidad;

public sealed class InMemoryCuentaRepository : ICuentaRepository
{
    private readonly Dictionary<Guid, Cuenta> _items = new();

    public Task<IReadOnlyCollection<Cuenta>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<Cuenta>>(_items.Values.ToList());

    public Task<Cuenta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.TryGetValue(id, out var cuenta) ? cuenta : null);

    public Task AddAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cuenta);

        _items[cuenta.Id] = cuenta;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cuenta);

        _items[cuenta.Id] = cuenta;
        return Task.CompletedTask;
    }
}
