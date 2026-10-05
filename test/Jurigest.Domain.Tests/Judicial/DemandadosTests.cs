using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Judicial;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class DemandadosTests
{
    [Theory]
    [InlineData("12.345.678-5", "12345678-5")]
    [InlineData("11.111.111-1", "11111111-1")]
    public void Rut_NormalizaYVerificaDigito(string entrada, string esperado)
    {
        Assert.Equal(esperado, RutChileno.Normalizar(entrada));
        Assert.Throws<ArgumentException>(() => RutChileno.Normalizar("12.345.678-9"));
    }

    [Fact]
    public void PersonaJuridica_RegistraRepresentanteYPermiteCompletarRut()
    {
        var causa = new Causa("C-102-26", "Tribunal", "Carátula");
        var principal = causa.AgregarDemandado("Empresa SpA", TipoPersonaDemandada.Juridica, true);
        principal.ActualizarIdentificacion("76.192.083-9", "Ana", "12.345.678-5");
        var aval = principal.AgregarAval("Garantía Ltda", TipoPersonaDemandada.Juridica,
            "11.111.111-1", "Pedro", "12.345.678-5");

        Assert.Equal("76192083-9", principal.Rut);
        Assert.Equal("Ana", principal.RepresentanteLegal);
        Assert.Equal("12345678-5", principal.RutRepresentanteLegal);
        Assert.Equal("11111111-1", aval.Rut);
        Assert.Equal("Pedro", aval.RepresentanteLegal);
        Assert.Throws<ArgumentException>(() => aval.ActualizarIdentificacion("11111111-2", "Pedro", null));
    }

    [Fact]
    public void PersonaNatural_NoAdmiteRepresentanteLegal()
    {
        var causa = new Causa("C-103-26", "Tribunal", "Carátula");
        Assert.Throws<ArgumentException>(() => causa.AgregarDemandado("Ana", TipoPersonaDemandada.Natural,
            true, "12345678-5", "Representante", null));
    }

    [Fact]
    public void Causa_AdmiteVariosDemandadosYMultiplesAvalesDelPrincipal()
    {
        var causa = new Causa("C-100-26", "Tribunal", "Carátula");
        var principal = causa.AgregarDemandado("Empresa SpA", TipoPersonaDemandada.Juridica, true);
        principal.AgregarAval("Ana", TipoPersonaDemandada.Natural);
        principal.AgregarAval("Otra Ltda", TipoPersonaDemandada.Juridica);
        causa.AgregarDemandado("Pedro", TipoPersonaDemandada.Natural, false);

        Assert.Equal(2, causa.Demandados.Count);
        Assert.Equal(2, principal.Avales.Count);
        Assert.Single(causa.Demandados, x => x.EsPrincipal);
    }

    [Fact]
    public void Causa_ImpideDosPrincipalesYAvalesEnDemandadoSecundario()
    {
        var causa = new Causa("C-101-26", "Tribunal", "Carátula");
        Assert.Throws<InvalidOperationException>(() => causa.AgregarDemandado("Secundario", TipoPersonaDemandada.Natural, false));
        causa.AgregarDemandado("Principal", TipoPersonaDemandada.Natural, true);
        Assert.Throws<InvalidOperationException>(() => causa.AgregarDemandado("Otro", TipoPersonaDemandada.Juridica, true));
        var secundario = causa.AgregarDemandado("Secundario", TipoPersonaDemandada.Natural, false);
        Assert.Throws<InvalidOperationException>(() => secundario.AgregarAval("Aval", TipoPersonaDemandada.Natural));
    }
}
