using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class DemandadosCausaTests
{
    [Fact]
    public async Task CausaExistente_AdmiteOtroDemandadoYAvalDelPrincipal()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, new
            {
                rit = "C-98766-26", tribunal = "Tribunal", descripcion = "Prueba / Empresa",
                fechaEncargoCausa = DateTime.Today,
                demandados = new[] { new { nombre = "Empresa SpA", tipoPersona = 2, esPrincipal = true,
                    rut = "76.192.083-9", representanteLegal = "Ana", rutRepresentanteLegal = "12.345.678-5",
                    avales = Array.Empty<object>() } }
            });
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var causaId = (await crear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var inicial = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            $"/api/Causas/{causaId}", admin.Token);
        var principalId = (await inicial.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("demandados")[0].GetProperty("id").GetGuid();

        using var agregar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            $"/api/Causas/{causaId}/demandados", admin.Token,
            new { nombre = "Pedro", tipoPersona = 1, esPrincipal = false, rut = "11.111.111-1" });
        Assert.Equal(HttpStatusCode.OK, agregar.StatusCode);
        var secundarioId = (await agregar.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var aval = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            $"/api/Causas/{causaId}/demandados/{principalId}/avales", admin.Token,
            new { nombre = "Ana", tipoPersona = 1, rut = "12.345.678-5" });
        Assert.Equal(HttpStatusCode.OK, aval.StatusCode);
        var avalId = (await aval.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        using var actualizarPrincipal = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{causaId}/demandados/{principalId}/identificacion", admin.Token,
            new { rut = "76.192.083-9", representanteLegal = "Beatriz", rutRepresentanteLegal = "12.345.678-5" });
        Assert.Equal(HttpStatusCode.NoContent, actualizarPrincipal.StatusCode);
        using var actualizarAval = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{causaId}/demandados/{principalId}/avales/{avalId}/identificacion", admin.Token,
            new { rut = "12.345.678-5" });
        Assert.Equal(HttpStatusCode.NoContent, actualizarAval.StatusCode);
        using var rutInvalido = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{causaId}/demandados/{secundarioId}/identificacion", admin.Token,
            new { rut = "11.111.111-2" });
        Assert.Equal(HttpStatusCode.BadRequest, rutInvalido.StatusCode);

        using var duplicarPrincipal = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            $"/api/Causas/{causaId}/demandados", admin.Token,
            new { nombre = "Otra empresa", tipoPersona = 2, esPrincipal = true, rut = "76.192.083-9" });
        Assert.Equal(HttpStatusCode.Conflict, duplicarPrincipal.StatusCode);
        using var avalSecundario = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            $"/api/Causas/{causaId}/demandados/{secundarioId}/avales", admin.Token,
            new { nombre = "Aval inválido", tipoPersona = 1, rut = "11.111.111-1" });
        Assert.Equal(HttpStatusCode.Conflict, avalSecundario.StatusCode);

        using var obtener = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            $"/api/Causas/{causaId}", admin.Token);
        var demandados = (await obtener.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("demandados");
        Assert.Equal(2, demandados.GetArrayLength());
        var principal = demandados.EnumerateArray().Single(x => x.GetProperty("esPrincipal").GetBoolean());
        Assert.Equal("76192083-9", principal.GetProperty("rut").GetString());
        Assert.Equal("Beatriz", principal.GetProperty("representanteLegal").GetString());
        Assert.Single(principal.GetProperty("avales").EnumerateArray());
    }

    [Fact]
    public async Task CrearCausa_PersistePrincipalSecundarioYDosAvales()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var body = new
        {
            rit = "C-98765-26", tribunal = "Tribunal", descripcion = "Prueba / Empresa",
            fechaEncargoCausa = DateTime.Today,
            demandados = new object[]
            {
                new { nombre = "Empresa SpA", tipoPersona = 2, esPrincipal = true,
                    avales = new[] { new { nombre = "Ana", tipoPersona = 1 }, new { nombre = "Garantía Ltda", tipoPersona = 2 } } },
                new { nombre = "Pedro", tipoPersona = 1, esPrincipal = false, avales = Array.Empty<object>() }
            }
        };
        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, body);
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var id = (await crear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var obtener = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            $"/api/Causas/{id}", admin.Token);
        Assert.Equal(HttpStatusCode.OK, obtener.StatusCode);
        var causa = await obtener.Content.ReadFromJsonAsync<JsonElement>();
        var demandados = causa.GetProperty("demandados");
        Assert.Equal(2, demandados.GetArrayLength());
        var principal = demandados.EnumerateArray().Single(x => x.GetProperty("esPrincipal").GetBoolean());
        Assert.Equal("Empresa SpA", principal.GetProperty("nombre").GetString());
        Assert.Equal(2, principal.GetProperty("avales").GetArrayLength());
    }
}
