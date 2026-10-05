using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class RolesCatalogoTests
{
    [Theory]
    [InlineData("Receptor")]
    [InlineData("Operador")]
    [InlineData("Digitador")]
    public async Task RolOperativo_SeAsignaYPuedeGestionarSinAdministrarUsuarios(string nombre)
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        using var crearRol = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/roles", admin.Token,
            new { nombre, perfil = 3 });
        Assert.Equal(HttpStatusCode.OK, crearRol.StatusCode);
        var id = (await crearRol.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var creado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/usuarios", admin.Token,
            new { nombre, email = "operativo@jurigest.test", password = "Operativo.Test.2026!", rol = 1, rolCatalogoId = id,
                rut = "15.003.942-8", telefono = "+56 9 5555 0202", direccion = "Calle Operativa 10", numeroOficina = "2" });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var usuario = await SeguridadTestHelper.IniciarSesionAsync(client, "operativo@jurigest.test", "Operativo.Test.2026!");
        using var lectura = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/Causas", usuario.Token);
        Assert.Equal(HttpStatusCode.OK, lectura.StatusCode);
        using var gestionar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/Catalogos/diligencias-realizadas", usuario.Token,
            new { nombre = "Gestión de prueba", codigoTipoDiligencia = 1 });
        Assert.Equal(HttpStatusCode.OK, gestionar.StatusCode);
        using var prohibido = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/seguridad/usuarios", usuario.Token);
        Assert.Equal(HttpStatusCode.Forbidden, prohibido.StatusCode);
        using var crearRolProhibido = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/roles", usuario.Token,
            new { nombre = "Escalada", perfil = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, crearRolProhibido.StatusCode);
        using var usuarios = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/seguridad/usuarios", admin.Token);
        var datos = await usuarios.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(nombre, datos.GetRawText());
    }

    [Fact]
    public async Task RolNuevo_ValidaDuplicadosYConservaPerfilConsultaAlAsignar()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        using var anonimo = await client.GetAsync("/api/seguridad/roles");
        Assert.Equal(HttpStatusCode.Unauthorized, anonimo.StatusCode);
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/roles", admin.Token,
            new { nombre = "Coordinador", perfil = 4 });
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var id = (await crear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var duplicado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/roles", admin.Token,
            new { nombre = " coordinador ", perfil = 4 });
        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
        using var invalido = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/roles", admin.Token,
            new { nombre = "Inválido", perfil = 999 });
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
        using var reservado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/roles", admin.Token,
            new { nombre = "Administrador", perfil = 4 });
        Assert.Equal(HttpStatusCode.BadRequest, reservado.StatusCode);
        using var desconocido = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/usuarios", admin.Token,
            new { nombre = "Sin rol", email = "sinrol@jurigest.test", password = "Consulta.Test.2026!", rol = 4, rolCatalogoId = Guid.NewGuid(),
                rut = "15.003.942-8", telefono = "+56 9 5555 0303", direccion = "Calle Consulta 20", numeroOficina = "3" });
        Assert.Equal(HttpStatusCode.BadRequest, desconocido.StatusCode);
        using var usuarioCreado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/usuarios", admin.Token,
            new { nombre = "Coordinador de pruebas", email = "coordinador@jurigest.test", password = "Consulta.Test.2026!", rol = 1, rolCatalogoId = id,
                rut = "15.003.942-8", telefono = "+56 9 5555 0303", direccion = "Calle Consulta 20", numeroOficina = "3" });
        Assert.Equal(HttpStatusCode.Created, usuarioCreado.StatusCode);
        var usuario = await SeguridadTestHelper.IniciarSesionAsync(client, "coordinador@jurigest.test", "Consulta.Test.2026!");
        using var lectura = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/Causas", usuario.Token);
        Assert.Equal(HttpStatusCode.OK, lectura.StatusCode);
        using var escritura = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/Catalogos/diligencias-realizadas", usuario.Token,
            new { nombre = "No permitido", codigoTipoDiligencia = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, escritura.StatusCode);
    }
}

