using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class CausaTests
{
    [Theory]
    [InlineData("C/123-2026")]
    [InlineData("C:123-2026")]
    [InlineData("C<123>-2026")]
    public void Crear_ConCaracteresInvalidosEnRit_LanzaExcepcion(string rit)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Causa(rit, "1° Juzgado Civil", "Persona A / Persona B"));

        Assert.StartsWith("El RIT contiene caracteres no permitidos.", excepcion.Message);
        Assert.Equal("rit", excepcion.ParamName);
    }

    [Fact]
    public void RegistrarGestion_ActualizaFechaYDiasSinGestion()
    {
        var fechaEncargo = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);
        var fechaGestion = fechaEncargo.AddDays(5);
        var causa = new Causa("C-123-2026", "1° Juzgado Civil", "Persona A / Persona B", fechaEncargo);

        causa.RegistrarGestion(fechaGestion);

        Assert.False(causa.EstaSinGestion);
        Assert.Equal(fechaGestion, causa.FechaGestionCausa);
        Assert.Equal(3, causa.ObtenerDiasSinGestion(fechaGestion.AddDays(3)));
    }

    [Fact]
    public void RegistrarGestion_AnteriorALaUltimaGestion_LanzaExcepcion()
    {
        var fechaEncargo = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);
        var causa = new Causa("C-123-2026", "1° Juzgado Civil", "Persona A / Persona B", fechaEncargo);
        causa.RegistrarGestion(fechaEncargo.AddDays(5));

        Assert.Throws<ArgumentException>(() => causa.RegistrarGestion(fechaEncargo.AddDays(4)));
    }
}
