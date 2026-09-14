using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class NormalizacionCausaTests
{
    [Fact]
    public async Task CrearYBuscarRolNumerico_NormalizaYRechazaTribunalEquivalente()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        object Cuerpo(string rol, string tribunal) => new { rit = rol, tribunal,
            descripcion = "Prueba normalización", fechaEncargoCausa = DateTime.Today };
        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, Cuerpo("11573-26", "1 Juzgado Civil de Santiago"));
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var id = (await crear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        foreach (var rol in new[] { "11573-26", "C-11573-26", "c-11573-26" })
        {
            using var buscar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
                $"/api/Causas/rit/{rol}", admin.Token);
            Assert.Equal(HttpStatusCode.OK, buscar.StatusCode);
            var causa = await buscar.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(id, causa.GetProperty("id").GetGuid());
            Assert.Equal("C-11573-26", causa.GetProperty("rit").GetString());
            Assert.Equal("1° Juzgado Civil de Santiago", causa.GetProperty("tribunal").GetString());
        }
        foreach (var tribunal in new[] { "1° Juzgado Civil de Santiago", "1º juzgado civil de Santiago" })
        {
            using var duplicado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
                "/api/Causas", admin.Token, Cuerpo("C-11573-26", tribunal));
            Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
        }
        using var otroTribunal = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, Cuerpo("11573-26", "2° Juzgado Civil de Santiago"));
        Assert.Equal(HttpStatusCode.OK, otroTribunal.StatusCode);
        using var exhorto = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, Cuerpo("E-11573-26", "1° Juzgado Civil de Santiago"));
        Assert.Equal(HttpStatusCode.OK, exhorto.StatusCode);
    }

    [Fact]
    public async Task Catalogo_ImpideAgregarVarianteOrdinalDelMismoTribunal()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        using var primero = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Catalogos/tribunales", admin.Token, new { nombre = "87 Juzgado Civil de Prueba" });
        Assert.Equal(HttpStatusCode.OK, primero.StatusCode);
        using var segundo = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Catalogos/tribunales", admin.Token, new { nombre = "87º juzgado civil de Prueba" });
        Assert.Equal(HttpStatusCode.Conflict, segundo.StatusCode);
    }
}