namespace Jurigest.Domain.Kernel.Abstractions;

/// <summary>
/// Define el contrato base para todas las entidades del dominio.
/// </summary>
/// <typeparam name="TId">Tipo del identificador de la entidad.</typeparam>
public interface IEntity<TId>
    where TId : notnull
{
    /// <summary>
    /// Obtiene el identificador único de la entidad.
    /// </summary>
    TId Id { get; }
}