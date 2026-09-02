using System.Net;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class VencimientosTests
{
    [Fact]
    public async Task BandejaPlazos_IncluyeDiligenciaActivaYCalculaVencimiento()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var crearCausa = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/Causas", admin.Token,
            new { id = Guid.Empty, rit = "C-PLAZO-001-2026", tribunal = "Tribunal de plazos", descripcion = "Control de vencimiento", fechaEncargoCausa = DateTime.UtcNow.Date });
        crearCausa.EnsureSuccessStatusCode();
        var causaId = await LeerGuidAsync(crearCausa, "id");

        using var crearDiligencia = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", admin.Token,
            new { descripcion = "Diligencia con alerta" });
        crearDiligencia.EnsureSuccessStatusCode();
        var diligenciaId = await LeerGuidAsync(crearDiligencia, "id");

        using var programar = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaId}/programar", admin.Token,
            new { fechaProgramada = DateTime.UtcNow.Date.AddDays(-2) });
        programar.EnsureSuccessStatusCode();

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Diligencias/plazos", admin.Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var alerta = documento.RootElement.EnumerateArray().Single(item => item.GetProperty("id").GetGuid() == diligenciaId);
        Assert.Equal(-2, alerta.GetProperty("diasParaVencimiento").GetInt32());
        Assert.Equal("C-PLAZO-001-2026", alerta.GetProperty("rit").GetString());
    }

    private static async Task<Guid> LeerGuidAsync(HttpResponseMessage response, string propiedad)
    {
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return documento.RootElement.GetProperty(propiedad).GetGuid();
    }
}
