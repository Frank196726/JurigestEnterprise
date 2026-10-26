using Jurigest.Domain.Contabilidad.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Contabilidad.Entities;

public class AsientoContable : AggregateRoot<Guid>
{
    private readonly List<MovimientoContable> _movimientos = [];

    protected AsientoContable()
    {
    }

    public AsientoContable(Guid id, string descripcion, DateTime? fecha = null)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        Descripcion = descripcion.Trim();
        Fecha = fecha ?? DateTime.UtcNow;
    }

    public string Descripcion { get; private set; } = string.Empty;

    public DateTime Fecha { get; private set; }

    public IReadOnlyCollection<MovimientoContable> Movimientos => _movimientos.AsReadOnly();

    public decimal TotalDebe => _movimientos.Where(m => m.Tipo == TipoMovimiento.Debito).Sum(m => m.Monto);

    public decimal TotalHaber => _movimientos.Where(m => m.Tipo == TipoMovimiento.Credito || m.Tipo == TipoMovimiento.Ajuste || m.Tipo == TipoMovimiento.Transferencia).Sum(m => m.Monto);

    public bool Cuadrado => Math.Abs(TotalDebe - TotalHaber) < 0.01m;

    public void AgregarMovimiento(MovimientoContable movimiento)
    {
        ArgumentNullException.ThrowIfNull(movimiento);

        if (_movimientos.Any(m => m.Id == movimiento.Id))
            throw new InvalidOperationException("El movimiento ya existe en el asiento.");

        movimiento.AsociarAlAsiento(Id);
        _movimientos.Add(movimiento);
    }
}

public class Asiento : AsientoContable
{
    public Asiento(Guid id, string descripcion, DateTime? fecha = null)
        : base(id, descripcion, fecha)
    {
    }
}
