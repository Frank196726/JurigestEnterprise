using System.Net;
using System.Net.Http.Json;
using Jurigest.Integration.Tests.Infrastructure;
namespace Jurigest.Integration.Tests.Judicial;
public sealed class MateriasTests
{
    [Fact]
    public async Task CrearMateria_LaIncluyeEnCatalogoYRechazaDuplicado()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        using var creada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Catalogos/materias", admin.Token, new { nombre = "Materia de prueba" });
        Assert.Equal(HttpStatusCode.OK, creada.StatusCode);
        using var listado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Catalogos/materias", admin.Token);
        listado.EnsureSuccessStatusCode();
        var materias = await listado.Content.ReadFromJsonAsync<System.Text.Json.JsonElement[]>();
        Assert.Contains(materias!, x => x.GetProperty("nombre").GetString() == "Materia de prueba");
        using var duplicada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Catalogos/materias", admin.Token, new { nombre = " Materia de prueba " });
        Assert.Equal(HttpStatusCode.Conflict, duplicada.StatusCode);
    }
}
