using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class DiligenciaTests
{
    [Fact]
    public void RegistrarResultado_CompletaLaDiligenciaYGuardaDatos()
    {
        var fechaGestion =
            new DateTime(2026, 8, 27, 15, 30, 0, DateTimeKind.Utc);

        var diligenciaRealizadaId = Guid.NewGuid();

        var diligencia =
            new Diligencia(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Notificación");

        diligencia.RegistrarResultado(
            diligenciaRealizadaId,
            "Notificación realizada",
            ResultadoDiligencia.Positiva,
            "Notificación entregada",
            "Estampe receptor",
            fechaGestion);

        Assert.Equal(
            diligenciaRealizadaId,
            diligencia.DiligenciaRealizadaId);

        Assert.Equal(
            "Notificación realizada",
            diligencia.DiligenciaRealizada);

        Assert.Equal(
            ResultadoDiligencia.Positiva,
            diligencia.Resultado);

        Assert.Equal(
            "Notificación entregada",
            diligencia.ResultadoDetalle);

        Assert.Equal(
            "Estampe receptor",
            diligencia.Estampe);

        Assert.Equal(
            fechaGestion,
            diligencia.FechaGestion);

        Assert.Equal(
            EstadoDiligencia.Completada,
            diligencia.Estado);
    }

    [Fact]
    public void RegistrarResultado_SinClasificacion_LanzaExcepcion()
    {
        var diligencia =
            new Diligencia(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Notificación");

        Assert.Throws<ArgumentException>(
            () => diligencia.RegistrarResultado(
                Guid.NewGuid(),
                "Notificación realizada",
                ResultadoDiligencia.SinResultado,
                "Detalle",
                "Estampe",
                DateTime.UtcNow));
    }
}
