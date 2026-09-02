using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class FlujoCausaDiligenciaResultadoTests
{
    [Fact]
    public async Task RegistrarResultado_SinAutenticacion_Devuelve401()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PutAsJsonAsync(
            $"/api/Diligencias/{Guid.NewGuid()}/resultado",
            new { resultado = 1, resultadoDetalle = "Detalle", estampe = "Estampe", fechaGestion = DateTime.UtcNow });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarResultado_SinDetalle_Devuelve400()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{Guid.NewGuid()}/resultado", admin.Token,
            new { resultado = 1, resultadoDetalle = "", estampe = "Estampe", fechaGestion = DateTime.UtcNow });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Administrador_CompletaFlujoYActualizaFechaGestionDeCausa()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var fechaEncargo = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);
        var fechaGestion = fechaEncargo.AddDays(4);

        using var crearCausa = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/Causas", admin.Token,
            new
            {
                id = Guid.Empty,
                rit = "C-FLUJO-001-2026",
                tribunal = "Tribunal de integración",
                descripcion = "Flujo integral",
                fechaEncargoCausa = fechaEncargo
            });
        Assert.Equal(HttpStatusCode.OK, crearCausa.StatusCode);
        var causaId = await LeerGuidAsync(crearCausa, "id");

        using var crearDiligencia = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", admin.Token,
            new { descripcion = "Notificación de prueba" });
        Assert.Equal(HttpStatusCode.OK, crearDiligencia.StatusCode);
        var diligenciaId = await LeerGuidAsync(crearDiligencia, "id");

        using var registrarResultado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaId}/resultado", admin.Token,
            new
            {
                resultado = 1,
                resultadoDetalle = "Notificación entregada",
                estampe = "Estampe de integración",
                fechaGestion
            });
        Assert.Equal(HttpStatusCode.OK, registrarResultado.StatusCode);

        using var obtenerCausas = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas", admin.Token);
        obtenerCausas.EnsureSuccessStatusCode();
        using var documento = JsonDocument.Parse(await obtenerCausas.Content.ReadAsStringAsync());
        var causa = documento.RootElement.EnumerateArray()
            .Single(elemento => elemento.GetProperty("id").GetGuid() == causaId);

        Assert.Equal(fechaGestion, causa.GetProperty("fechaGestionCausa").GetDateTime());
        var diasEsperados = Math.Max(0, (DateTime.UtcNow.Date - fechaGestion.Date).Days);
        Assert.Equal(diasEsperados, causa.GetProperty("diasSinGestion").GetInt32());
    }

    private static async Task<Guid> LeerGuidAsync(HttpResponseMessage response, string propiedad)
    {
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return documento.RootElement.GetProperty(propiedad).GetGuid();
    }
}
