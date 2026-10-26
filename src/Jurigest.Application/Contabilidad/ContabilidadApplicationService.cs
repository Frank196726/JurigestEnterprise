using Jurigest.Domain.Contabilidad.Entities;
using Jurigest.Domain.Contabilidad.Enums;

namespace Jurigest.Application.Contabilidad;

public sealed class ContabilidadApplicationService
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IAsientoRepository _asientoRepository;

    public ContabilidadApplicationService(ICuentaRepository cuentaRepository, IAsientoRepository asientoRepository)
    {
        _cuentaRepository = cuentaRepository;
        _asientoRepository = asientoRepository;
    }

    public async Task<IReadOnlyCollection<Cuenta>> ObtenerCuentasAsync(CancellationToken cancellationToken = default)
        => await _cuentaRepository.GetAllAsync(cancellationToken);

    public async Task<Cuenta?> ObtenerCuentaAsync(Guid id, CancellationToken cancellationToken = default)
        => await _cuentaRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Cuenta> CrearCuentaAsync(string codigo, string nombre, decimal saldoInicial = 0m, CancellationToken cancellationToken = default)
    {
        var cuenta = new Cuenta(Guid.NewGuid(), codigo, nombre, saldoInicial);
        await _cuentaRepository.AddAsync(cuenta, cancellationToken);
        return cuenta;
    }

    public async Task<Cuenta> RegistrarMovimientoAsync(Guid cuentaId, decimal monto, string descripcion, TipoMovimiento tipo, CancellationToken cancellationToken = default)
    {
        var cuenta = await _cuentaRepository.GetByIdAsync(cuentaId, cancellationToken)
            ?? throw new InvalidOperationException("La cuenta no existe.");

        if (tipo == TipoMovimiento.Debito)
            cuenta.Debitar(monto, descripcion);
        else
            cuenta.Acreditar(monto, descripcion);

        await _cuentaRepository.UpdateAsync(cuenta, cancellationToken);
        return cuenta;
    }

    public async Task<IReadOnlyCollection<AsientoContable>> ObtenerAsientosAsync(CancellationToken cancellationToken = default)
        => await _asientoRepository.GetAllAsync(cancellationToken);

    public async Task<AsientoContable> CrearAsientoAsync(string descripcion, CancellationToken cancellationToken = default)
    {
        var asiento = new AsientoContable(Guid.NewGuid(), descripcion);
        await _asientoRepository.AddAsync(asiento, cancellationToken);
        return asiento;
    }

    public async Task<AsientoContable> AgregarMovimientoAsientoAsync(Guid asientoId, Guid cuentaId, decimal monto, string descripcion, TipoMovimiento tipo, CancellationToken cancellationToken = default)
    {
        var asiento = await _asientoRepository.GetByIdAsync(asientoId, cancellationToken)
            ?? throw new InvalidOperationException("El asiento no existe.");

        var movimiento = new MovimientoContable(Guid.NewGuid(), cuentaId, monto, tipo, descripcion);
        asiento.AgregarMovimiento(movimiento);

        await _asientoRepository.UpdateAsync(asiento, cancellationToken);
        return asiento;
    }
}
