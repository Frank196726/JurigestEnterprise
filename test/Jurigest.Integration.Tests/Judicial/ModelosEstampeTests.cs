using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class ModelosEstampeTests
{
    [Fact]
    public async Task Administrador_CreaEditaYDesactivaModeloIndependiente()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/modelos-estampe", admin.Token, new { nombre = "Modelo integración",
                tipoDiligencia = 1, resultado = 1, contenido = "ROL {{ROL}}", activo = true });
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var id = (await crear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        using var listar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/modelos-estampe?tipoDiligencia=1&resultado=1", admin.Token);
        Assert.Contains((await listar.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray(),
            x => x.GetProperty("id").GetGuid() == id);

        using var editar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/modelos-estampe/{id}", admin.Token, new { nombre = "Modelo integración editado",
                tipoDiligencia = 1, resultado = 2, contenido = "CERTIFICO {{RESULTADO_DETALLE}}", activo = true });
        Assert.Equal(HttpStatusCode.OK, editar.StatusCode);

        using var eliminar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Delete,
            $"/api/modelos-estampe/{id}", admin.Token);
        Assert.Equal(HttpStatusCode.NoContent, eliminar.StatusCode);
        using var activos = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/modelos-estampe", admin.Token);
        Assert.DoesNotContain((await activos.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray(),
            x => x.GetProperty("id").GetGuid() == id);
    }

    [Fact]
    public async Task Administrador_CreaModeloParaNuevoTipoDeDiligencia()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        using var tipo = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Catalogos/tipos-diligencia", admin.Token, new { nombre = "Diligencia modelo personalizada" });
        Assert.Equal(HttpStatusCode.OK, tipo.StatusCode);
        var codigo = (await tipo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("codigoSistema").GetInt32();
        Assert.True(codigo >= 100);
        using var gestion = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Catalogos/diligencias-realizadas", admin.Token,
            new { nombre = "Gestión personalizada relacionada", codigoTipoDiligencia = codigo });
        Assert.Equal(HttpStatusCode.OK, gestion.StatusCode);
        var gestionId = (await gestion.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var modelo = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/modelos-estampe", admin.Token, new { nombre = "Modelo personalizado",
                tipoDiligencia = codigo, diligenciaRealizadaId = gestionId,
                contenido = "ROL {{ROL}}", activo = true });
        Assert.Equal(HttpStatusCode.OK, modelo.StatusCode);
        var creado = await modelo.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(codigo, creado.GetProperty("tipoDiligencia").GetInt32());
        Assert.Equal(gestionId, creado.GetProperty("diligenciaRealizadaId").GetGuid());
    }
}
