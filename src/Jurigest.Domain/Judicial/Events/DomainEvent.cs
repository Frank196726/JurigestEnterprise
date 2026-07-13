namespace Jurigest.Domain.Kernel.Events;

/// <summary>
/// Clase base para los eventos del dominio.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }

    public DateTime OccurredOn { get; }
}