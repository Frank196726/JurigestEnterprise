using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

/// <summary>
/// Representa una resolución judicial.
/// </summary>
public sealed class Resolucion : Entity<Guid>
{
    private Resolucion()
    {
    }

    public Resolucion(
        Guid id,
        TipoResolucion tipo,
        DateTime fecha,
        string descripcion)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        Tipo = tipo;
        Fecha = fecha;
        Descripcion = descripcion.Trim();
    }

    public TipoResolucion Tipo { get; private set; }

    public DateTime Fecha { get; private set; }

    public string Descripcion { get; private set; }
}