using System.Net;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class HistorialCausaTests
{
    [Fact]
    public async Task ObtenerCausaPorId_EntregaDatosNecesariosParaHistorial()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client,
            SeguridadTestHelper.AdminEmail,
            SeguridadTestHelper.AdminPassword);

        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            "/api/Causas",
            admin.Token,
            new
            {
                id = Guid.Empty,
                rit = "C-HIST-001-2026",
                tribunal = "Tribunal historial",
                descripcion = "Causa con trazabilidad",
                fechaEncargoCausa = DateTime.UtcNow.Date.AddDays(-3)
            });
        crear.EnsureSuccessStatusCode();
        using var creado = JsonDocument.Parse(
            await crear.Content.ReadAsStringAsync());
        var id = creado.RootElement.GetProperty("id").GetGuid();

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Get,
            $"/api/Causas/{id}",
            admin.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal("C-HIST-001-2026", json.RootElement.GetProperty("rit").GetString());
        Assert.True(json.RootElement.TryGetProperty("fechaEncargoCausa", out _));
        Assert.True(json.RootElement.TryGetProperty("diasSinGestion", out _));
    }

    [Fact]
    public async Task ObtenerCausaPorId_Inexistente_Devuelve404()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client,
            SeguridadTestHelper.AdminEmail,
            SeguridadTestHelper.AdminPassword);
        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Get,
            $"/api/Causas/{Guid.NewGuid()}",
            admin.Token);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
