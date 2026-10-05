using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class ReciboDetalleTests
{
    private static Recibo Crear() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        Guid.NewGuid(), "Embargo vehículo", 57000m, new DateTime(2026, 9, 29));

    [Fact]
    public void CompletarDatos_SumaGestionYAdicionalesYConservaReferencia()
    {
        var recibo = Crear();
        recibo.CompletarDatos("Abogada", "Receptor", "9087-26", "13°", "Banco / Morales",
            "Embargo vehículo", "OP-123", 100000m, "Not. Rvm.", "Traslado", 27000m, 30000m);
        Assert.Equal(57000m, recibo.Monto);
        Assert.Equal(30000m, recibo.ValorGestion);
        Assert.Equal(27000m, recibo.TotalAdicionales);
        Assert.Equal("OP-123", recibo.NumeroOperacion);
        Assert.Equal(100000m, recibo.Cuantia);
        Assert.Equal("Not. Rvm.", recibo.Observacion);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0.5)]
    public void CompletarDatos_RechazaAdicionalesInvalidos(decimal adicionales)
    {
        var recibo = Crear();
        Assert.Throws<ArgumentException>(() => recibo.CompletarDatos(null, null, null, null,
            null, null, null, null, null, null, adicionales, 30000m));
        Assert.Equal(57000m, recibo.Monto);
    }

    [Fact]
    public void CompletarDatos_RechazaTotalFueraDeRango()
    {
        var recibo = Crear();
        Assert.Throws<ArgumentException>(() => recibo.CompletarDatos(null, null, null, null,
            null, null, null, null, null, null, 1m, MontoEstampe.Maximo));
    }
}
