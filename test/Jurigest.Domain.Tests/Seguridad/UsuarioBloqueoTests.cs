using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;

namespace Jurigest.Domain.Tests.Seguridad;

public sealed class UsuarioBloqueoTests
{
    [Fact]
    public void QuintoIntentoFallido_BloqueaDuranteQuinceMinutos()
    {
        var usuario = CrearUsuario();
        var fechaUtc =
            new DateTime(2026, 8, 16, 1, 0, 0, DateTimeKind.Utc);

        for (var intento = 1; intento <= 4; intento++)
        {
            var fueBloqueado =
                usuario.RegistrarIntentoFallido(
                    fechaUtc,
                    5,
                    TimeSpan.FromMinutes(15));

            Assert.False(fueBloqueado);
            Assert.Equal(
                intento,
                usuario.IntentosFallidos);
        }

        var quintoIntento =
            usuario.RegistrarIntentoFallido(
                fechaUtc,
                5,
                TimeSpan.FromMinutes(15));

        Assert.True(quintoIntento);
        Assert.True(usuario.EstaBloqueado(fechaUtc));
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Equal(
            fechaUtc.AddMinutes(15),
            usuario.BloqueadoHastaUtc);
    }

    [Fact]
    public void CambiarPasswordHash_EliminaElBloqueo()
    {
        var usuario = CrearUsuario();
        var fechaUtc =
            new DateTime(2026, 8, 16, 1, 0, 0, DateTimeKind.Utc);

        for (var intento = 1; intento <= 5; intento++)
        {
            usuario.RegistrarIntentoFallido(
                fechaUtc,
                5,
                TimeSpan.FromMinutes(15));
        }

        usuario.CambiarPasswordHash("hash-nuevo");

        Assert.False(usuario.EstaBloqueado(fechaUtc));
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHastaUtc);
    }

    [Fact]
    public void BloqueoExpirado_NuevoFalloReiniciaElContador()
    {
        var usuario = CrearUsuario();
        var fechaUtc =
            new DateTime(2026, 8, 16, 1, 0, 0, DateTimeKind.Utc);

        for (var intento = 1; intento <= 5; intento++)
        {
            usuario.RegistrarIntentoFallido(
                fechaUtc,
                5,
                TimeSpan.FromMinutes(15));
        }

        var fechaPosterior =
            fechaUtc.AddMinutes(16);

        var fueBloqueado =
            usuario.RegistrarIntentoFallido(
                fechaPosterior,
                5,
                TimeSpan.FromMinutes(15));

        Assert.False(fueBloqueado);
        Assert.False(usuario.EstaBloqueado(fechaPosterior));
        Assert.Equal(1, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHastaUtc);
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario(
            Guid.NewGuid(),
            "Usuario de prueba",
            "usuario@jurigest.test",
            "hash-inicial",
            RolUsuario.Procurador);
    }
}