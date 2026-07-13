using Jurigest.Domain.Kernel.Events;

namespace Jurigest.Domain.Kernel.Common;

/// <summary>
/// Representa la raíz de un agregado del dominio.
/// </summary>
/// <typeparam name="TId">Tipo del identificador.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Eventos de dominio pendientes de publicar.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}