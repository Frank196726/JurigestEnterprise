using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class ReceptorRenombradoTests
{
    [Fact]
    public async Task DiligenciaGuardaIdDelReceptor_YLaVisibilidadSobreviveAlRenombrarlo()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        var receptorId = await SeguridadTestHelper.CrearReceptorAsync(client, admin.Token, "Receptor Original");
        using var causaResponse = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/Causas", admin.Token,
            new
            {
                id = Guid.Empty,
                rit = "C-REN-001-2026",
                tribunal = "1° Juzgado Civil de Santiago",
                descripcion = "Demandante / Demandado",
                fechaEncargoCausa = DateTime.UtcNow.Date
            });
        causaResponse.EnsureSuccessStatusCode();
        var causaId = (await causaResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var diligenciaResponse = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", admin.Token,
            new { descripcion = "Notificación", tipo = 1 });
        diligenciaResponse.EnsureSuccessStatusCode();
        var diligenciaId = (await diligenciaResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var asignar = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaId}/asignar-receptor", admin.Token,
            new { receptorJudicial = "Receptor Original" });
        asignar.EnsureSuccessStatusCode();

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            var diligencia = await db.Diligencias.SingleAsync(x => x.Id == diligenciaId);
            Assert.Equal(receptorId, diligencia.ReceptorJudicialId);

            // Renombrar el receptor del catálogo no toca el texto histórico de la diligencia.
            var receptor = await db.ReceptoresJudiciales.SingleAsync(x => x.Id == receptorId);
            receptor.CambiarNombre("Receptor Renombrado");
            await db.SaveChangesAsync();
        }

        var usuarioId = await SeguridadTestHelper.CrearProcuradorAsync(client, admin.Token);
        using var asociar = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/seguridad/usuarios/{usuarioId}", admin.Token,
            new
            {
                nombre = "Procurador de pruebas",
                email = SeguridadTestHelper.ProcuradorEmail,
                rut = "15.003.942-8",
                telefono = "+56 9 5555 0101",
                direccion = "Avenida de Pruebas 123",
                numeroOficina = "Oficina 7",
                receptorIds = new[] { receptorId }
            });
        asociar.EnsureSuccessStatusCode();
        var usuario = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.ProcuradorEmail, SeguridadTestHelper.ProcuradorPassword);

        using var causas = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas", usuario.Token);
        var ids = (await causas.Content.ReadFromJsonAsync<JsonElement>())
            .EnumerateArray().Select(x => x.GetProperty("id").GetGuid());
        Assert.Contains(causaId, ids);
        using var detalleDiligencia = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/Diligencias/{diligenciaId}", usuario.Token);
        Assert.Equal(HttpStatusCode.OK, detalleDiligencia.StatusCode);
    }
}
