using Jurigest.Domain.Seguridad;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;

namespace Jurigest.Domain.Tests.Seguridad;

public sealed class PerfilOperadorTests
{
    [Fact]
    public void Operador_TieneLecturaEscrituraDeCausasYGestionDeDiligencias()
    {
        var permisos = PermisosSistema.Predeterminados(RolUsuario.Operador);

        Assert.Equal(
            new[]
            {
                "CausasLectura", "CausasEscritura", "DiligenciasLectura", "DiligenciasGestion",
                "DocumentosLectura", "ResolucionesLectura"
            },
            permisos);
    }

    [Fact]
    public void RolPersonalizado_NoPuedeLlamarseComoElPerfilOperador()
    {
        Assert.Throws<ArgumentException>(() => new RolCatalogo(Guid.NewGuid(), "operador", RolUsuario.Procurador));
    }

    [Fact]
    public void RolPersonalizado_PuedeUsarPerfilBaseOperador()
    {
        var rol = new RolCatalogo(Guid.NewGuid(), "Digitador", RolUsuario.Operador);

        Assert.Equal(RolUsuario.Operador, rol.Perfil);
    }
}
