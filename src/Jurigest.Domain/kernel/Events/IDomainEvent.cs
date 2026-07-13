namespace Jurigest.Domain.Kernel.Events;

/// <summary>
/// Representa un evento ocurrido dentro del dominio.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Fecha y hora en que ocurrió el evento.
    /// </summary>
    DateTime OccurredOn { get; }
}