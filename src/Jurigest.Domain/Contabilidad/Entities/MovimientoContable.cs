using Jurigest.Domain.Contabilidad.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Contabilidad.Entities;

public class MovimientoContable : Entity<Guid>
{
    protected MovimientoContable()
    {
    }

    public MovimientoContable(Guid id, Guid cuentaId, decimal monto, TipoMovimiento tipo, string descripcion, DateTime? fecha = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (monto <= 0)
            throw new ArgumentOutOfRangeException(nameof(monto), "El monto debe ser mayor que cero.");

        CuentaId = cuentaId;
        Monto = monto;
        Tipo = tipo;
        Descripcion = descripcion.Trim();
        Fecha = fecha ?? DateTime.UtcNow;
    }

    public Guid CuentaId { get; private set; }

    public Guid? AsientoId { get; private set; }

    public decimal Monto { get; private set; }

    public TipoMovimiento Tipo { get; private set; }

    public string Descripcion { get; private set; } = string.Empty;

    public DateTime Fecha { get; private set; }

    public void CambiarDescripcion(string nuevaDescripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nuevaDescripcion);
        Descripcion = nuevaDescripcion.Trim();
    }

    public void AsociarAlAsiento(Guid asientoId)
    {
        AsientoId = asientoId;
    }
}

public class Movimiento : MovimientoContable
{
    public Movimiento(Guid id, Guid cuentaId, decimal monto, TipoMovimiento tipo, string descripcion, DateTime? fecha = null)
        : base(id, cuentaId, monto, tipo, descripcion, fecha)
    {
    }
}
