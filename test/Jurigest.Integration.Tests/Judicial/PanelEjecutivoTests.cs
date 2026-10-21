using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class PanelEjecutivoTests
{
    [Fact]
    public async Task TiempoSinGestionar_UsaFechaGestionDelResultadoRegistrado()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        var hoy = DateTime.UtcNow.Date;
        var gestionada = new Causa("C-66001-26", "1° Juzgado Civil de Santiago", "A / B", hoy.AddDays(-30));
        gestionada.AgregarDiligencia("Notificación de demanda").RegistrarResultado(
            Guid.NewGuid(), "Notificación personal", ResultadoDiligencia.Positiva,
            null, "Estampe", hoy.AddDays(-2));
        var sinGestion = new Causa("C-66002-26", "1° Juzgado Civil de Santiago", "C / D", hoy.AddDays(-20));
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.AddRange(gestionada, sinGestion);
            await db.SaveChangesAsync();
        }

        using var respuesta = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/panel-ejecutivo", admin.Token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var tramos = (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("causasPorAntiguedad");
        // La gestionada cuenta desde la fecha de gestión (2 días), no desde el encargo.
        Assert.Equal(1, tramos.GetProperty("hasta10Dias").GetInt32());
        Assert.Equal(0, tramos.GetProperty("entre11Y15Dias").GetInt32());
        // Sin gestiones, cuenta desde la fecha de encargo (20 días).
        Assert.Equal(1, tramos.GetProperty("masDe15Dias").GetInt32());
    }
}
