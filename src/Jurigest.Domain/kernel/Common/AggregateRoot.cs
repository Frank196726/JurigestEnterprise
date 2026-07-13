namespace Jurigest.Domain.Kernel.Common;

/// <summary>
/// Representa la raíz de un agregado del dominio.
/// </summary>
/// <typeparam name="TId">Tipo del identificador.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }
}