using Jurigest.Domain.Contabilidad.Entities;
using Jurigest.Domain.Contabilidad.Enums;

namespace Jurigest.Domain.Tests;

public class ContabilidadTests
{
    [Fact]
    public void Cuenta_AcreditarYDebitar_ActualizaSaldoCorrectamente()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), "1101", "Caja chica", 100m);

        cuenta.Acreditar(50m, "Ingreso");
        cuenta.Debitar(30m, "Egreso");

        Assert.Equal(120m, cuenta.Saldo);
    }

    [Fact]
    public void AsientoContable_ConMovimientosCuadrados_SeConsideraCuadrado()
    {
        var asiento = new AsientoContable(Guid.NewGuid(), "Diario");
        asiento.AgregarMovimiento(new MovimientoContable(Guid.NewGuid(), Guid.NewGuid(), 100m, TipoMovimiento.Debito, "Debe"));
        asiento.AgregarMovimiento(new MovimientoContable(Guid.NewGuid(), Guid.NewGuid(), 100m, TipoMovimiento.Credito, "Haber"));

        Assert.True(asiento.Cuadrado);
    }
}
