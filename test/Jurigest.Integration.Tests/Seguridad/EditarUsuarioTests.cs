using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;
namespace Jurigest.Integration.Tests.Seguridad;
public sealed class EditarUsuarioTests
{
    [Fact]
    public async Task Edicion_PersisteDatos_ValidaDuplicados_YExigeAdministrador()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var id = await SeguridadTestHelper.CrearProcuradorAsync(client, admin.Token);
        var usuario = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.ProcuradorEmail, SeguridadTestHelper.ProcuradorPassword);
        object Datos(string email) => new { nombre = "Nombre actualizado", email, rut = "15.003.942-8", telefono = "999999999", direccion = "Nueva dirección 30", numeroOficina = "15" };
        using var prohibido = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put, $"/api/seguridad/usuarios/{id}", usuario.Token, Datos("nuevo@test.cl"));
        Assert.Equal(HttpStatusCode.Forbidden, prohibido.StatusCode);
        using var duplicado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put, $"/api/seguridad/usuarios/{id}", admin.Token, Datos(SeguridadTestHelper.AdminEmail));
        Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
        using var guardado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put, $"/api/seguridad/usuarios/{id}", admin.Token, Datos("nuevo@test.cl"));
        Assert.Equal(HttpStatusCode.OK, guardado.StatusCode);
        using var consulta = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, $"/api/seguridad/usuarios/{id}", admin.Token);
        var detalle = await consulta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Nombre actualizado", detalle.GetProperty("nombre").GetString());
        Assert.Equal("nuevo@test.cl", detalle.GetProperty("email").GetString());
        Assert.Equal("Nueva dirección 30", detalle.GetProperty("direccion").GetString());
        Assert.Equal("15", detalle.GetProperty("numeroOficina").GetString());
        using var antiguo = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/Causas", usuario.Token);
        Assert.Equal(HttpStatusCode.Unauthorized, antiguo.StatusCode);
    }
}
