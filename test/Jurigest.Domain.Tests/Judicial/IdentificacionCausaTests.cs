using Jurigest.Domain.Judicial;
using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class IdentificacionCausaTests
{
    [Theory]
    [InlineData("11573-26", "C-11573-26")]
    [InlineData(" 11573 - 26 ", "C-11573-26")]
    [InlineData("c-11573-26", "C-11573-26")]
    [InlineData("e-1370-26", "E-1370-26")]
    [InlineData("C-TEST-2026", "C-TEST-2026")]
    public void Rol_NormalizaSinDuplicarNiCambiarPrefijoExplicito(string entrada, string esperado)
    {
        Assert.Equal(esperado, IdentificacionCausa.NormalizarRol(entrada));
        Assert.Equal(esperado, IdentificacionCausa.NormalizarRol(esperado));
    }

    [Theory]
    [InlineData("1", "1°")]
    [InlineData("1°", "1°")]
    [InlineData("2º", "2°")]
    [InlineData("01 Juzgado Civil de Santiago", "1° Juzgado Civil de Santiago")]
    [InlineData(" 2  º   Juzgado Civil de Santiago ", "2° Juzgado Civil de Santiago")]
    [InlineData("23° Juzgado Civil de Santiago", "23° Juzgado Civil de Santiago")]
    [InlineData("Juzgado de Letras", "Juzgado de Letras")]
    public void Tribunal_FormatoOrdinalUnico(string entrada, string esperado)
    {
        Assert.Equal(esperado, IdentificacionCausa.NormalizarTribunal(entrada));
        Assert.Equal(esperado, IdentificacionCausa.NormalizarTribunal(esperado));
    }

    [Fact]
    public void Equivalencia_ConservaNumeroCompetenciaYCiudad()
    {
        Assert.True(IdentificacionCausa.MismoTribunal("1 juzgado civil de Santiago", "1° Juzgado Civil de Santiago"));
        Assert.False(IdentificacionCausa.MismoTribunal("1 Juzgado Civil de Santiago", "2° Juzgado Civil de Santiago"));
        Assert.False(IdentificacionCausa.MismoTribunal("1 Juzgado Civil de Santiago", "1° Juzgado Civil de Valparaíso"));
        Assert.False(IdentificacionCausa.MismoTribunal("1 Juzgado Civil", "1° Juzgado de Familia"));
        var causa = new Causa("11573-26", "1 Juzgado Civil de Santiago", "A / B");
        Assert.Equal("C-11573-26", causa.Rit);
        Assert.Equal("1° Juzgado Civil de Santiago", causa.Tribunal);
    }
}