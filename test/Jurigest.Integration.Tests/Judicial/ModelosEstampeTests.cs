using Jurigest.Integration.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class ModelosEstampeTests
{
    [Fact]
    public async Task ModelosBusquedaCompartenGestionConDosClasificaciones()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        Guid? gestionId = null;
        foreach (var resultado in new[] { 1, 2 })
        {
            using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
                "/api/modelos-estampe", admin.Token, new
                {
                    nombre = "Búsqueda variante " + resultado,
                    tipoDiligencia = 112,
                    resultado,
                    contenido = "CERTIFICO: Modelo de búsqueda"
                });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var modelo = await response.Content.ReadFromJsonAsync<JsonElement>();
            var vinculada = modelo.GetProperty("diligenciaRealizadaId").GetGuid();
            if (gestionId.HasValue) Assert.Equal(gestionId.Value, vinculada);
            gestionId = vinculada;
        }
        foreach (var tipo in new[] { 105, 112 })
        {
            using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
                "/api/Catalogos/diligencias-realizadas?codigoTipoDiligencia=" + tipo, admin.Token);
            var gestion = Assert.Single((await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
            Assert.Equal("Búsqueda", gestion.GetProperty("nombre").GetString());
            Assert.Equal(gestionId.GetValueOrDefault(), gestion.GetProperty("id").GetGuid());
        }
    }

    [Theory]
    [InlineData(105)]
    [InlineData(112)]
    public async Task BusquedaSeGuardaConEncargosExistentes(int tipo)
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        using var modelo = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/modelos-estampe", admin.Token, new { nombre = "Búsqueda Conserje", tipoDiligencia = 112, resultado = 2, contenido = "CERTIFICO: Búsqueda" });
        modelo.EnsureSuccessStatusCode();
        var gestionId = (await modelo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("diligenciaRealizadaId").GetGuid();
        var causa = new Jurigest.Domain.Judicial.Entities.Causa("C-123-26", "Tribunal", "Banco / Persona", DateTime.UtcNow);
        var diligencia = causa.AgregarDiligencia("Búsqueda");
        diligencia.CambiarTipo((Jurigest.Domain.Judicial.Enums.TipoDiligencia)tipo);
        using (var scope = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.CreateScope(factory.Services))
        {
            var db = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Jurigest.Persistence.Context.JurigestDbContext>(scope.ServiceProvider);
            db.Causas.Add(causa);
            await db.SaveChangesAsync();
        }
        using var guardado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Diligencias/{diligencia.Id}/resultado", admin.Token,
            new { diligenciaRealizadaId = gestionId, resultado = 2, fechaGestion = DateTime.UtcNow, estampe = "CERTIFICO: Búsqueda", monto = 0 });
        Assert.Equal(HttpStatusCode.OK, guardado.StatusCode);
    }

    [Fact]
    public async Task Administrador_CreaEditaYDesactivaModeloIndependiente()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/modelos-estampe", admin.Token, new
            {
                nombre = "Modelo integración",
                tipoDiligencia = 1,
                resultado = 1,
                contenido = "ROL {{ROL}}",
                activo = true
            });
        Assert.Equal(HttpStatusCode.OK, crear.StatusCode);
        var creadoModelo = await crear.Content.ReadFromJsonAsync<JsonElement>();
        var id = creadoModelo.GetProperty("id").GetGuid();
        var gestionId = creadoModelo.GetProperty("diligenciaRealizadaId").GetGuid();
        using var gestiones = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Catalogos/diligencias-realizadas?codigoTipoDiligencia=1", admin.Token);
        Assert.Contains((await gestiones.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray(),
            x => x.GetProperty("id").GetGuid() == gestionId && x.GetProperty("nombre").GetString() == "Modelo integración");

        using var listar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/modelos-estampe?tipoDiligencia=1&resultado=1", admin.Token);
        Assert.Contains((await listar.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray(),
            x => x.GetProperty("id").GetGuid() == id);

        using var editar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/modelos-estampe/{id}", admin.Token, new
            {
                nombre = "Modelo integración editado",
                tipoDiligencia = 1,
                resultado = 2,
                contenido = "CERTIFICO {{RESULTADO_DETALLE}}",
                activo = true
            });
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
            "/api/modelos-estampe", admin.Token, new
            {
                nombre = "Modelo personalizado",
                tipoDiligencia = codigo,
                diligenciaRealizadaId = gestionId,
                contenido = "ROL {{ROL}}",
                activo = true
            });
        Assert.Equal(HttpStatusCode.OK, modelo.StatusCode);
        var creado = await modelo.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(codigo, creado.GetProperty("tipoDiligencia").GetInt32());
        Assert.Equal(gestionId, creado.GetProperty("diligenciaRealizadaId").GetGuid());
    }
}
