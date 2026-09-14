using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class CorreccionIngresoTests
{
    private static Causa Crear() => new("C-8771-26", "Tribunal", "A / B");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PrimeraDiligencia_PermiteCorregirInclusoConResultado(bool conResultado)
    {
        var causa = Crear();
        var primera = causa.AgregarDiligencia("Notificación");
        if (conResultado)
            primera.RegistrarResultado((ResultadoDiligencia)1, "Resultado", "Estampe", DateTime.Today);
        var estado = primera.Estado;
        var fechaGestion = primera.FechaGestion;

        causa.CorregirIngreso("Tribunal corregido", "Demandante / Demandado", primera.Id,
            "Embargo", TipoDiligencia.Embargo, DateTime.Today.AddDays(2));

        Assert.Equal("C-8771-26", causa.Rit);
        Assert.Equal("Demandante / Demandado", causa.Descripcion);
        Assert.Equal("Embargo", primera.Descripcion);
        Assert.Equal(TipoDiligencia.Embargo, primera.Tipo);
        Assert.Equal(DateTime.Today.AddDays(2), primera.FechaProgramada);
        Assert.Equal(estado, primera.Estado);
        Assert.Equal(fechaGestion, primera.FechaGestion);
        Assert.Equal(conResultado ? "Estampe" : null, primera.Estampe);
    }

    [Fact]
    public void SegundaDiligencia_BloqueaIngresoSinDependerDeFechasNiResultados()
    {
        var causa = Crear();
        var primera = causa.AgregarDiligencia("Primera");
        var segunda = causa.AgregarDiligencia("Segunda");
        segunda.Programar(DateTime.Today.AddDays(-10));
        Assert.False(causa.PuedeCorregirIngreso);
        Assert.Throws<InvalidOperationException>(() => causa.CorregirIngreso(
            "Otro tribunal", "X / Y", primera.Id, "Embargo", TipoDiligencia.Embargo, DateTime.Today));
        Assert.Throws<InvalidOperationException>(() => causa.ActualizarDatos("Tribunal", "X / Y"));
        Assert.Throws<InvalidOperationException>(() => causa.ValidarCorreccionDiligencia(primera.Id));
        causa.ValidarCorreccionDiligencia(segunda.Id);
        Assert.Equal("A / B", causa.Descripcion);
        Assert.Equal("Primera", primera.Descripcion);
    }

    [Fact]
    public void SinDiligencias_PermiteSoloCorregirCaratula()
    {
        var causa = Crear();
        causa.CorregirIngreso("Tribunal", "X / Y", null, null, null, null);
        Assert.Equal("X / Y", causa.Descripcion);
        Assert.Throws<ArgumentException>(() => causa.CorregirIngreso(
            "Tribunal", "Otra", Guid.NewGuid(), "Encargo", null, DateTime.Today));
    }

    [Fact]
    public void DatosInvalidos_NoModificanElIngreso()
    {
        var causa = Crear();
        var primera = causa.AgregarDiligencia("Primera");
        Assert.Throws<ArgumentException>(() => causa.CorregirIngreso(
            "Otro", "X / Y", primera.Id, "Encargo", TipoDiligencia.Embargo, null));
        Assert.Equal("A / B", causa.Descripcion);
        Assert.Equal("Primera", primera.Descripcion);
        Assert.Throws<InvalidOperationException>(() => causa.ActualizarDatos("C-OTRO", "Tribunal", "X / Y"));
        Assert.Equal("C-8771-26", causa.Rit);
    }
}