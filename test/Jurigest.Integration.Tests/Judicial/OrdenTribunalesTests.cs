using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class OrdenTribunalesTests
{
    [Fact]
    public async Task ObtenerTribunales_OrdenaPorNumeroCorrelativo()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            foreach (var nombre in new[] { "10° Juzgado Civil de Santiago", "Tribunal de Prueba",
                         "2° Juzgado Civil de Santiago", "1° Juzgado Civil de Santiago", "30° Juzgado Civil de Santiago" })
                db.Tribunales.Add(new TribunalCatalogo(Guid.NewGuid(), nombre));
            await db.SaveChangesAsync();
        }

        using var respuesta = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Catalogos/tribunales", admin.Token);
        respuesta.EnsureSuccessStatusCode();
        var nombres = (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray()
            .Select(x => x.GetProperty("nombre").GetString())
            .ToList();

        Assert.Equal(new[]
        {
            "1° Juzgado Civil de Santiago", "2° Juzgado Civil de Santiago",
            "10° Juzgado Civil de Santiago", "30° Juzgado Civil de Santiago", "Tribunal de Prueba"
        }, nombres);
    }
}
