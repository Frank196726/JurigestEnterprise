using Jurigest.Domain.Contabilidad.Enums;
using Jurigest.Domain.Contabilidad.ValueObjects;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Contabilidad.Entities;

public class Cuenta : AggregateRoot<Guid>
{
    private readonly List<MovimientoContable> _movimientos = [];

    protected Cuenta()
    {
    }

    public Cuenta(Guid id, string codigo, string nombre, decimal saldoInicial = 0m)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
        Saldo = saldoInicial;
    }

    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public decimal Saldo { get; private set; }

    public IReadOnlyCollection<MovimientoContable> Movimientos => _movimientos.AsReadOnly();

    public void Acreditar(decimal monto, string descripcion)
    {
        RegistrarMovimiento(new MovimientoContable(Guid.NewGuid(), Id, monto, TipoMovimiento.Credito, descripcion));
    }

    public void Debitar(decimal monto, string descripcion)
    {
        RegistrarMovimiento(new MovimientoContable(Guid.NewGuid(), Id, monto, TipoMovimiento.Debito, descripcion));
    }

    public void RegistrarMovimiento(MovimientoContable movimiento)
    {
        ArgumentNullException.ThrowIfNull(movimiento);

        if (movimiento.CuentaId != Id)
            throw new InvalidOperationException("El movimiento no pertenece a esta cuenta.");

        _movimientos.Add(movimiento);

        if (movimiento.Tipo == TipoMovimiento.Debito)
            Saldo -= movimiento.Monto;
        else if (movimiento.Tipo == TipoMovimiento.Credito || movimiento.Tipo == TipoMovimiento.Ajuste || movimiento.Tipo == TipoMovimiento.Transferencia)
            Saldo += movimiento.Monto;
    }
}

public class CuentaContable : Cuenta
{
    public CuentaContable(Guid id, string codigo, string nombre, decimal saldoInicial = 0m)
        : base(id, codigo, nombre, saldoInicial)
    {
    }
}
