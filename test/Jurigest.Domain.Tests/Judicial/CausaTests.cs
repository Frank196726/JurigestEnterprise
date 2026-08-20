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

        Assert.Equal("El RIT contiene caracteres no permitidos.", excepcion.Message);
    }
}
