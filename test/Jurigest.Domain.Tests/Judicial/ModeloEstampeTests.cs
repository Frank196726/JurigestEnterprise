using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class ModeloEstampeTests
{
    [Fact]
    public void CrearModelo_ConservaPlantillaIndependiente()
    {
        var modelo = new ModeloEstampe(Guid.NewGuid(), "Notificación positiva",
            TipoDiligencia.Notificacion, "ROL: {{ROL}}\nCERTIFICO: {{RESULTADO}}",
            ResultadoDiligencia.Positiva);

        Assert.Equal("Notificación positiva", modelo.Nombre);
        Assert.Equal(TipoDiligencia.Notificacion, modelo.TipoDiligencia);
        Assert.Equal(ResultadoDiligencia.Positiva, modelo.Resultado);
        Assert.True(modelo.Activo);
    }

    [Fact]
    public void CrearModelo_RechazaContenidoVacioOExcesivo()
    {
        Assert.Throws<ArgumentException>(() => new ModeloEstampe(Guid.NewGuid(), "Modelo",
            TipoDiligencia.Notificacion, ""));
        Assert.Throws<ArgumentException>(() => new ModeloEstampe(Guid.NewGuid(), "Modelo",
            TipoDiligencia.Notificacion, new string('X', 8001)));
    }

    [Fact]
    public void CrearModelo_AdmiteTipoPersonalizadoDelCatalogo()
    {
        var modelo = new ModeloEstampe(Guid.NewGuid(), "Tipo personalizado",
            (TipoDiligencia)100, "Contenido");
        Assert.Equal(100, (int)modelo.TipoDiligencia);
    }
}
