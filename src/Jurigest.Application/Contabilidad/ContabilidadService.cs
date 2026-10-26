using Jurigest.Domain.Contabilidad.Entities;
using Jurigest.Domain.Contabilidad.Enums;

namespace Jurigest.Application.Contabilidad;

public class ContabilidadService
{
    public Cuenta CrearCuenta(Guid id, string codigo, string nombre, decimal saldoInicial = 0m)
        => new(id, codigo, nombre, saldoInicial);

    public MovimientoContable RegistrarMovimiento(Guid cuentaId, decimal monto, string descripcion, TipoMovimiento tipo)
        => new(Guid.NewGuid(), cuentaId, monto, tipo, descripcion);

    public decimal ConsultarSaldo(Cuenta cuenta)
    {
        ArgumentNullException.ThrowIfNull(cuenta);
        return cuenta.Saldo;
    }
}
