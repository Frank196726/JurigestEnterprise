using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class ReciboTests
{
    [Fact]
    public void CrearRecibo_IniciaPendienteYConservaDatosHistoricos()
    {
        var reciboId = Guid.NewGuid();
        var causaId = Guid.NewGuid();
        var diligenciaId = Guid.NewGuid();
        var diligenciaRealizadaId = Guid.NewGuid();
        var fechaEmision = new DateTime(2026, 9, 20, 15, 30, 0);

        var recibo = new Recibo(
            reciboId,
            causaId,
            diligenciaId,
            diligenciaRealizadaId,
            "Notificación personal",
            60000m,
            fechaEmision);

        Assert.Equal(reciboId, recibo.Id);
        Assert.Equal(causaId, recibo.CausaId);
        Assert.Equal(diligenciaId, recibo.DiligenciaId);
        Assert.Equal(diligenciaRealizadaId, recibo.DiligenciaRealizadaId);
        Assert.Equal("Notificación personal", recibo.DiligenciaRealizada);
        Assert.Equal(60000m, recibo.Monto);
        Assert.Equal(EstadoRecibo.Pendiente, recibo.Estado);
        Assert.Equal(fechaEmision, recibo.FechaEmision);
        Assert.Null(recibo.FechaPago);
    }

    [Fact]
    public void MarcarPagado_CambiaEstadoYFechaSinModificarDatosHistoricos()
    {
        var causaId = Guid.NewGuid();
        var diligenciaId = Guid.NewGuid();
        var diligenciaRealizadaId = Guid.NewGuid();
        var fechaEmision = new DateTime(2026, 9, 20, 15, 30, 0);
        var fechaPago = new DateTime(2026, 9, 21, 10, 0, 0);

        var recibo = new Recibo(
            Guid.NewGuid(),
            causaId,
            diligenciaId,
            diligenciaRealizadaId,
            "Notificación personal",
            60000m,
            fechaEmision);

        recibo.MarcarPagado(fechaPago);

        Assert.Equal(EstadoRecibo.Pagado, recibo.Estado);
        Assert.Equal(fechaPago, recibo.FechaPago);

        Assert.Equal(causaId, recibo.CausaId);
        Assert.Equal(diligenciaId, recibo.DiligenciaId);
        Assert.Equal(diligenciaRealizadaId, recibo.DiligenciaRealizadaId);
        Assert.Equal("Notificación personal", recibo.DiligenciaRealizada);
        Assert.Equal(60000m, recibo.Monto);
        Assert.Equal(fechaEmision, recibo.FechaEmision);
    }

    [Fact]
    public void MarcarPagado_SegundaVez_ConservaFechaPagoOriginal()
    {
        var fechaEmision = new DateTime(2026, 9, 20, 15, 30, 0);
        var primeraFechaPago = new DateTime(2026, 9, 21, 10, 0, 0);
        var segundaFechaPago = new DateTime(2026, 9, 22, 12, 0, 0);

        var recibo = new Recibo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Notificación personal",
            60000m,
            fechaEmision);

        recibo.MarcarPagado(primeraFechaPago);
        recibo.MarcarPagado(segundaFechaPago);

        Assert.Equal(EstadoRecibo.Pagado, recibo.Estado);
        Assert.Equal(primeraFechaPago, recibo.FechaPago);
        Assert.Equal(60000m, recibo.Monto);
    }

    [Fact]
    public void MarcarPagado_FechaAnteriorAEmision_LanzaExcepcion()
    {
        var fechaEmision = new DateTime(2026, 9, 20, 15, 30, 0);

        var recibo = new Recibo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Notificación personal",
            60000m,
            fechaEmision);

        var excepcion = Assert.Throws<InvalidOperationException>(
            () => recibo.MarcarPagado(
                fechaEmision.AddMinutes(-1)));

        Assert.Equal(
            "La fecha de pago no puede ser anterior a la fecha de emisión.",
            excepcion.Message);

        Assert.Equal(EstadoRecibo.Pendiente, recibo.Estado);
        Assert.Null(recibo.FechaPago);
        Assert.Equal(60000m, recibo.Monto);
    }
}
