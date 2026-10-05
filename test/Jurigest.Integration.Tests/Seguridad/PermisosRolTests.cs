using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class PermisosRolTests
{
    [Fact]
    public async Task RevocacionYConcesionSeAplicanASesionAbiertaYNoPermitenEscalada()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        await SeguridadTestHelper.CrearProcuradorAsync(client, admin.Token);
        var usuario = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.ProcuradorEmail, SeguridadTestHelper.ProcuradorPassword);
        async Task<HttpResponseMessage> Get(string token, string ruta) => await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, ruta, token);
        async Task<HttpResponseMessage> Put(string token, string clave, string[] seleccion, Guid version) => await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put, $"/api/seguridad/permisos/{clave}", token, new { seleccion, version });
        using var original = await Get(usuario.Token, "/api/Causas");
        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        using var prohibido = await Put(usuario.Token, "Procurador", [], Guid.Empty);
        Assert.Equal(HttpStatusCode.Forbidden, prohibido.StatusCode);
        using var adminBloqueado = await Put(admin.Token, "Administrador", [], Guid.Empty);
        Assert.Equal(HttpStatusCode.BadRequest, adminBloqueado.StatusCode);
        using var escalada = await Put(admin.Token, "Procurador", ["Administracion"], Guid.Empty);
        Assert.Equal(HttpStatusCode.BadRequest, escalada.StatusCode);
        using var dependencia = await Put(admin.Token, "Procurador", ["CausasEscritura"], Guid.Empty);
        Assert.Equal(HttpStatusCode.BadRequest, dependencia.StatusCode);
        using var revocado = await Put(admin.Token, "Procurador", [], Guid.Empty);
        Assert.Equal(HttpStatusCode.OK, revocado.StatusCode);
        var version = (await revocado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("version").GetGuid();
        using var denegado = await Get(usuario.Token, "/api/Causas");
        Assert.Equal(HttpStatusCode.Forbidden, denegado.StatusCode);
        using var antiguo = await Put(admin.Token, "Procurador", ["CausasLectura"], Guid.Empty);
        Assert.Equal(HttpStatusCode.Conflict, antiguo.StatusCode);
        using var concedido = await Put(admin.Token, "Procurador", ["CausasLectura", "CausasEscritura"], version);
        Assert.Equal(HttpStatusCode.OK, concedido.StatusCode);
        using var permitido = await Get(usuario.Token, "/api/Causas");
        Assert.Equal(HttpStatusCode.OK, permitido.StatusCode);
        using var propios = await Get(usuario.Token, "/api/seguridad/permisos/mios");
        Assert.Contains("CausasEscritura", (await propios.Content.ReadFromJsonAsync<string[]>())!);
        using var administracion = await Get(usuario.Token, "/api/seguridad/usuarios");
        Assert.Equal(HttpStatusCode.Forbidden, administracion.StatusCode);
        using var nuevoAdministrador = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/seguridad/roles", admin.Token, new { nombre = "Superusuario", perfil = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, nuevoAdministrador.StatusCode);
        using var auditoria = await Get(admin.Token, "/api/seguridad/permisos");
        Assert.Equal(HttpStatusCode.OK, auditoria.StatusCode);
    }
}
