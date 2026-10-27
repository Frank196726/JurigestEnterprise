using Jurigest.Domain.Judicial.Entities;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class BuscarRolVariosTribunalesTests
{
    [Fact]
    public async Task BuscarTodasPorRit_DevuelveUnaCausaPorTribunal()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var primera = new Causa("C-55001-26", "10° Juzgado Civil de Santiago", "A / B");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.Add(primera);
            await db.SaveChangesAsync();
        }
        var segunda = new Causa("C-55001-26", "2° Juzgado Civil de Santiago", "C / D");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.Add(segunda);
            db.Causas.Add(new Causa("C-55002-26", "2° Juzgado Civil de Santiago", "Otra / Causa"));
            await db.SaveChangesAsync();
        }

        // Acepta el ROL sin prefijo, como la búsqueda rápida.
        using var respuesta = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/rit/55001-26/causas", admin.Token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var causas = (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        Assert.Equal(2, causas.Count);
        Assert.Equal(primera.Id, causas[0].GetProperty("id").GetGuid());
        Assert.Equal(segunda.Id, causas[1].GetProperty("id").GetGuid());
        Assert.Equal("2° Juzgado Civil de Santiago", causas[1].GetProperty("tribunal").GetString());
        Assert.Equal("C", causas[1].GetProperty("demandante").GetString());

        // El endpoint de una sola causa sigue devolviendo la más antigua.
        using var una = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/rit/C-55001-26", admin.Token);
        Assert.Equal(primera.Id, (await una.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task BuscarTodasPorRit_CualquierLetra_IncluyeOtrasLetrasDelMismoNumero()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var ejecutiva = new Causa("E-55003-26", "1° Juzgado Civil de Santiago", "E / F");
        var civil = new Causa("C-55003-26", "23° Juzgado Civil de Santiago", "A / B");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.Add(ejecutiva);
            db.Causas.Add(civil);
            db.Causas.Add(new Causa("C-155003-26", "2° Juzgado Civil de Santiago", "Otro / Numero"));
            db.Causas.Add(new Causa("C-55003-25", "2° Juzgado Civil de Santiago", "Otro / Año"));
            await db.SaveChangesAsync();
        }

        using var respuesta = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/rit/C-55003-26/causas?cualquierLetra=true", admin.Token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var causas = (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray().ToList();
        // Primero la del ROL buscado y luego la de otra letra; no entran otro número ni otro año.
        Assert.Equal([civil.Id, ejecutiva.Id], causas.Select(x => x.GetProperty("id").GetGuid()));

        // Sin el parámetro, solo el ROL exacto.
        using var exacta = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/rit/C-55003-26/causas", admin.Token);
        Assert.Equal([civil.Id], (await exacta.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray()
            .Select(x => x.GetProperty("id").GetGuid()));
    }

    [Fact]
    public async Task BuscarTodasPorRit_SinCausas_DevuelveListaVacia()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var respuesta = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/rit/C-NO-EXISTE-26/causas", admin.Token);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Empty((await respuesta.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
    }
}
