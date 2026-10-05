using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class NormalizacionCausaTests
{
    [Fact]
    public async Task BusquedaRapida_EntregaPartesYDireccionDeLaDiligencia()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, new
            {
                rit = "C-44551-26", tribunal = "3° Juzgado Civil de Santiago",
                descripcion = "Banco de Prueba / Apellido anterior", fechaEncargoCausa = DateTime.Today,
                demandados = new[] { new { nombre = "María Elena Leiva Soto", tipoPersona = 1, esPrincipal = true } }
            });
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var causaId = (await crear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var crearDiligencia = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            $"/api/Causas/{causaId}/diligencias", admin.Token,
            new { descripcion = "Notificación de demanda", tipo = 1 });
        Assert.Equal(HttpStatusCode.OK, crearDiligencia.StatusCode);
        var diligenciaId = (await crearDiligencia.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var actualizar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Diligencias/{diligenciaId}", admin.Token,
            new { descripcion = "Notificación de demanda", tipo = 1,
                direccion = "Los Magnolios 6945", comuna = "Peñalolén" });
        Assert.Equal(HttpStatusCode.OK, actualizar.StatusCode);

        using var buscar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/rit/C-44551-26", admin.Token);
        Assert.Equal(HttpStatusCode.OK, buscar.StatusCode);
        var resultado = await buscar.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("María Elena Leiva Soto", resultado.GetProperty("nombreDemandado").GetString());
        Assert.Equal("Los Magnolios 6945", resultado.GetProperty("direccion").GetString());
        Assert.Equal("Peñalolén", resultado.GetProperty("comuna").GetString());
        Assert.Equal("Banco de Prueba", resultado.GetProperty("demandante").GetString());
        Assert.Equal("María Elena Leiva Soto", resultado.GetProperty("demandado").GetString());
    }

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
