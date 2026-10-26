using Jurigest.Application.Judicial;
using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Tests;

public sealed class FiltroReceptoresTests
{
    private static readonly FiltroReceptores Filtro =
        new(["Receptor Uno", "Receptor Dos"]);

    private static Causa CrearCausa(params string?[] receptores)
    {
        var causa = new Causa("C-123-2026", "1° Juzgado Civil", "Persona A / Persona B");
        foreach (var receptor in receptores)
        {
            var diligencia = causa.AgregarDiligencia("Notificación");
            if (receptor is not null) diligencia.AsignarReceptor(receptor);
        }
        return causa;
    }

    [Fact]
    public void PermiteCausa_ConDiligenciaDeCualquieraDeSusReceptores()
    {
        Assert.True(Filtro.PermiteCausa(CrearCausa("Receptor Uno").Diligencias));
        Assert.True(Filtro.PermiteCausa(CrearCausa("Otro", " receptor dos ").Diligencias));
    }

    [Fact]
    public void PermiteCausa_SinReceptorAsignado()
    {
        Assert.True(Filtro.PermiteCausa(CrearCausa().Diligencias));
        Assert.True(Filtro.PermiteCausa(CrearCausa(null, null).Diligencias));
    }

    [Fact]
    public void NoPermiteCausa_AsignadaSoloAReceptoresAjenos()
    {
        Assert.False(Filtro.PermiteCausa(CrearCausa("Otro", null).Diligencias));
    }

    [Fact]
    public void FiltrarDiligencias_IncluyePropiasYSinReceptorDeCausasVisibles()
    {
        var visible = CrearCausa("Receptor Uno", null, "Otro");
        var ajena = CrearCausa("Otro", null);

        var resultado = Filtro.FiltrarDiligencias(visible.Diligencias.Concat(ajena.Diligencias));

        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, d => Assert.Equal(visible.Id, d.CausaId));
        Assert.DoesNotContain(resultado, d => d.ReceptorJudicial == "Otro");
    }

    [Fact]
    public void FiltrarCausas_ConservaCausasSinDiligencias()
    {
        var vacia = CrearCausa();
        var ajena = CrearCausa("Otro");

        var resultado = Filtro.FiltrarCausas([vacia, ajena], ajena.Diligencias);

        Assert.Equal([vacia.Id], resultado.Select(c => c.Id));
    }
}
