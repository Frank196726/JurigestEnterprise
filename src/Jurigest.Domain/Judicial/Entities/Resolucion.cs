using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class Resolucion : Entity<Guid>
{
    private Resolucion()
    {
    }

    public Resolucion(
        Guid id,
        Guid causaId,
        TipoResolucion tipo,
        DateTime fecha,
        string descripcion)
        : base(id)
    {
        if (causaId == Guid.Empty)
            throw new ArgumentException("La causa es obligatoria.");

        if (fecha == default)
            throw new ArgumentException("La fecha es obligatoria.");

        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        CausaId = causaId;
        Tipo = tipo;
        Fecha = fecha;
        Descripcion = descripcion.Trim();
    }

    public Guid CausaId { get; private set; }

    public TipoResolucion Tipo { get; private set; }

    public DateTime Fecha { get; private set; }

    public string Descripcion { get; private set; } = string.Empty;
}