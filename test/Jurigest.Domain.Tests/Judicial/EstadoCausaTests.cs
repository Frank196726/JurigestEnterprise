using Jurigest.Domain.Judicial;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Tests.Judicial;

public sealed class EstadoCausaTests
{
    private static readonly DateTime FechaEncargo =
        new(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);

    private static Causa CrearCausa() =>
        new("C-123-2026", "1° Juzgado Civil", "Persona A / Persona B", FechaEncargo);

    [Fact]
    public void CausaNueva_QuedaIngresada()
    {
        Assert.Equal(EstadoCausa.Ingresada, CrearCausa().Estado);
    }

    [Fact]
    public void PrimeraGestion_PasaLaCausaATramitacion()
    {
        var causa = CrearCausa();

        causa.RegistrarGestion(FechaEncargo.AddDays(1));

        Assert.Equal(EstadoCausa.EnTramitacion, causa.Estado);
    }

    [Fact]
    public void Gestion_EnCausaEnTramitacion_MantieneElEstado()
    {
        var causa = CrearCausa();
        causa.CambiarEstado(EstadoCausa.EnTramitacion);

        causa.RegistrarGestion(FechaEncargo.AddDays(1));

        Assert.Equal(EstadoCausa.EnTramitacion, causa.Estado);
    }

    [Theory]
    [InlineData(EstadoCausa.Suspendida, "suspendida")]
    [InlineData(EstadoCausa.Terminada, "terminada")]
    [InlineData(EstadoCausa.Archivada, "archivada")]
    public void CausaCerrada_NoAdmiteDiligenciasNiGestiones(EstadoCausa estado, string nombre)
    {
        var causa = CrearCausa();
        causa.CambiarEstado(estado);

        Assert.False(causa.AdmiteDiligencias);
        var alAgregar = Assert.Throws<CausaNoAdmiteDiligenciasException>(
            () => causa.AgregarDiligencia("Notificación"));
        Assert.Contains(nombre, alAgregar.Message);
        Assert.Throws<CausaNoAdmiteDiligenciasException>(
            () => causa.RegistrarGestion(FechaEncargo.AddDays(1)));
        Assert.Empty(causa.Diligencias);
        Assert.Null(causa.FechaGestionCausa);
    }

    [Theory]
    [InlineData(EstadoCausa.Ingresada)]
    [InlineData(EstadoCausa.EnTramitacion)]
    public void OtrosEstados_AdmitenDiligencias(EstadoCausa estado)
    {
        var causa = CrearCausa();
        causa.CambiarEstado(estado);

        Assert.True(causa.AdmiteDiligencias);
        Assert.NotNull(causa.AgregarDiligencia("Notificación"));
    }

    [Fact]
    public void CausaReactivada_VuelveAAdmitirDiligencias()
    {
        var causa = CrearCausa();
        causa.CambiarEstado(EstadoCausa.Suspendida);

        causa.CambiarEstado(EstadoCausa.EnTramitacion);

        Assert.NotNull(causa.AgregarDiligencia("Notificación"));
    }

    [Theory]
    [InlineData(EstadoCausa.EnTramitacion)]
    [InlineData(EstadoCausa.Suspendida)]
    [InlineData(EstadoCausa.Terminada)]
    [InlineData(EstadoCausa.Archivada)]
    public void CambiarEstado_AplicaElEstadoIndicado(EstadoCausa estado)
    {
        var causa = CrearCausa();

        causa.CambiarEstado(estado);

        Assert.Equal(estado, causa.Estado);
    }

    [Fact]
    public void CambiarEstado_AIngresadaSinGestiones_EstaPermitido()
    {
        var causa = CrearCausa();
        causa.CambiarEstado(EstadoCausa.Suspendida);

        causa.CambiarEstado(EstadoCausa.Ingresada);

        Assert.Equal(EstadoCausa.Ingresada, causa.Estado);
    }

    [Fact]
    public void CambiarEstado_AIngresadaConGestiones_LanzaExcepcion()
    {
        var causa = CrearCausa();
        causa.RegistrarGestion(FechaEncargo.AddDays(1));

        Assert.Throws<InvalidOperationException>(
            () => causa.CambiarEstado(EstadoCausa.Ingresada));
        Assert.Equal(EstadoCausa.EnTramitacion, causa.Estado);
    }

    [Fact]
    public void CambiarEstado_ValorInexistente_LanzaExcepcion()
    {
        var causa = CrearCausa();

        Assert.Throws<ArgumentException>(
            () => causa.CambiarEstado((EstadoCausa)99));
    }
}
