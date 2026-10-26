using Jurigest.Application.Judicial;
using Jurigest.Domain.Judicial.Entities;

namespace Jurigest.Application.Tests;

public sealed class FiltroReceptoresTests
{
    private static readonly Guid UnoId = Guid.NewGuid();

    private static readonly FiltroReceptores Filtro =
        new([UnoId], ["Receptor Uno", "Receptor Dos"]);

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

    [Fact]
    public void AsignacionVinculada_SeComparaPorIdAunqueCambieElNombre()
    {
        // Receptor propio renombrado: el texto histórico ya no coincide, el Id sí.
        Assert.True(Filtro.PermiteCausa([(UnoId, "Nombre anterior")]));
        // Receptor ajeno con un nombre igual a uno propio: decide el Id.
        Assert.False(Filtro.PermiteCausa([(Guid.NewGuid(), "Receptor Uno")]));
        // Texto libre fuera del catálogo: se compara por nombre.
        Assert.True(Filtro.PermiteCausa([(null, "receptor dos")]));
    }
}
