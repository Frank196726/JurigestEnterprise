using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class CorreccionIngresoTests
{
    [Fact]
    public async Task Corregir_PersisteCamposYBloqueaTrasCrearSegundaDiligencia()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var causa = new Causa("C-CORREGIR-26", "Tribunal", "A / B");
        var primera = causa.AgregarDiligencia("Notificación histórica");
        var encargo = new DiligenciaEncargadaCatalogo(Guid.NewGuid(), "Embargo de prueba", 3);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.Add(causa);
            db.Set<DiligenciaEncargadaCatalogo>().Add(encargo);
            await db.SaveChangesAsync();
        }

        var payload = new { tribunal = "Tribunal corregido", descripcion = "Demandante / Demandado",
            diligenciaId = primera.Id, diligenciaEncargadaId = encargo.Id,
            fechaProgramada = new DateTime(2026, 9, 15), rit = "C-NO-CAMBIAR" };
        using var guardar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{causa.Id}", admin.Token, payload);
        Assert.Equal(HttpStatusCode.OK, guardar.StatusCode);
        using var antes = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            $"/api/Causas/{causa.Id}", admin.Token);
        var detalle = await antes.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(detalle.GetProperty("puedeCorregirIngreso").GetBoolean());
        Assert.Equal("C-CORREGIR-26", detalle.GetProperty("rit").GetString());
        Assert.Equal("Embargo de prueba", detalle.GetProperty("diligenciaEncargada").GetString());
        Assert.Equal(payload.fechaProgramada, detalle.GetProperty("fechaProgramada").GetDateTime());
        Assert.Equal(payload.descripcion, detalle.GetProperty("descripcion").GetString());

        using var crearSegunda = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            $"/api/Causas/{causa.Id}/diligencias", admin.Token, new { descripcion = "Segunda" });
        Assert.Equal(HttpStatusCode.OK, crearSegunda.StatusCode);
        var segunda = (await crearSegunda.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var bloqueo = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{causa.Id}", admin.Token, payload);
        Assert.Equal(HttpStatusCode.Conflict, bloqueo.StatusCode);
        using var bloqueoLegado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{causa.Id}", admin.Token, new { tribunal = "Otro", descripcion = "Alterada" });
        Assert.Equal(HttpStatusCode.Conflict, bloqueoLegado.StatusCode);

        foreach (var ruta in new[] { "programar", "tipo", "" })
        {
            var body = new { fechaProgramada = DateTime.Today, tipo = 1, descripcion = "Alterada" };
            using var alternativa = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
                $"/api/Diligencias/{primera.Id}" + (ruta.Length == 0 ? "" : $"/{ruta}"), admin.Token, body);
            Assert.Equal(HttpStatusCode.Conflict, alternativa.StatusCode);
        }
        using var programarSegunda = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Diligencias/{segunda}/programar", admin.Token, new { fechaProgramada = DateTime.Today });
        Assert.Equal(HttpStatusCode.OK, programarSegunda.StatusCode);

        using var despues = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            $"/api/Causas/{causa.Id}", admin.Token);
        detalle = await despues.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(detalle.GetProperty("puedeCorregirIngreso").GetBoolean());
        Assert.Equal(payload.descripcion, detalle.GetProperty("descripcion").GetString());
        Assert.Equal(payload.fechaProgramada, detalle.GetProperty("fechaProgramada").GetDateTime());
        await using var verificacion = factory.Services.CreateAsyncScope();
        var persistida = await verificacion.ServiceProvider.GetRequiredService<JurigestDbContext>()
            .Diligencias.AsNoTracking().SingleAsync(d => d.Id == primera.Id);
        Assert.Equal(3, (int)persistida.Tipo);
    }

    [Theory]
    [InlineData("otra-diligencia")]
    [InlineData("catalogo-invalido")]
    [InlineData("sin-fecha")]
    [InlineData("caratula-larga")]
    public async Task SolicitudInvalida_NoGuardaCambiosParciales(string caso)
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var causa = new Causa("C-INVALIDO-26", "Tribunal", "A / B");
        var primera = causa.AgregarDiligencia("Primera");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.Add(causa);
            await db.SaveChangesAsync();
        }
        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{causa.Id}", admin.Token, new {
                tribunal = "Otro", descripcion = caso == "caratula-larga" ? new string('X', 501) : "X / Y",
                diligenciaId = caso == "otra-diligencia" ? Guid.NewGuid() : primera.Id,
                diligenciaEncargadaId = caso == "catalogo-invalido" ? (Guid?)Guid.NewGuid() : null,
                fechaProgramada = caso == "sin-fecha" ? (DateTime?)null : DateTime.Today });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var comprobacion = factory.Services.CreateAsyncScope();
        var actual = await comprobacion.ServiceProvider.GetRequiredService<JurigestDbContext>()
            .Causas.AsNoTracking().Include(c => c.Diligencias).SingleAsync(c => c.Id == causa.Id);
        Assert.Equal("A / B", actual.Descripcion);
        Assert.Equal("Tribunal", actual.Tribunal);
        Assert.Equal("Primera", actual.Diligencias.Single().Descripcion);
        Assert.Null(actual.Diligencias.Single().FechaProgramada);
    }

    [Fact]
    public async Task SinAutenticacion_NoPermiteCorregir()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PutAsJsonAsync($"/api/Causas/{Guid.NewGuid()}",
            new { tribunal = "Tribunal", descripcion = "A / B" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}