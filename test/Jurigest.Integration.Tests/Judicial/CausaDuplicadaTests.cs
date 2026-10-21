using Jurigest.Domain.Judicial;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class CausaDuplicadaTests
{
    [Fact]
    public async Task Crear_MismoRolYTribunalConOtrasMayusculas_DevuelveConflictoConMensaje()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        object Body(string tribunal) => new
        {
            rit = "C-77001-26",
            tribunal,
            descripcion = "Demandante / Demandado",
            fechaEncargoCausa = DateTime.Today
        };

        using var primera = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, Body("1° Juzgado Civil de Santiago"));
        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);
        using var duplicada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, Body("1 JUZGADO CIVIL DE SANTIAGO"));

        Assert.Equal(HttpStatusCode.Conflict, duplicada.StatusCode);
        Assert.Equal(IdentificacionCausa.MensajeCausaDuplicada, await LeerMensajeAsync(duplicada));

        // Un mismo ROL puede existir en otro tribunal.
        using var otroTribunal = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            "/api/Causas", admin.Token, Body("2° Juzgado Civil de Santiago"));
        Assert.Equal(HttpStatusCode.OK, otroTribunal.StatusCode);
    }

    [Fact]
    public async Task Actualizar_TribunalAlDeOtraCausaConMismoRol_DevuelveConflictoYNoGuarda()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client,
            SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var existente = new Causa("C-77002-26", "2° Juzgado Civil de Santiago", "A / B");
        var editada = new Causa("C-77002-26", "3° Juzgado Civil de Santiago", "A / B");
        editada.AgregarDiligencia("Notificación");
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.Causas.AddRange(existente, editada);
            await db.SaveChangesAsync();
        }

        using var duplicada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{editada.Id}", admin.Token, new
            {
                tribunal = "2 juzgado civil de santiago",
                descripcion = "A / B"
            });

        Assert.Equal(HttpStatusCode.Conflict, duplicada.StatusCode);
        Assert.Equal(IdentificacionCausa.MensajeCausaDuplicada, await LeerMensajeAsync(duplicada));
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            var actual = await db.Causas.AsNoTracking().SingleAsync(x => x.Id == editada.Id);
            Assert.Equal("3° Juzgado Civil de Santiago", actual.Tribunal);
        }

        // Cambiar solo las mayúsculas del propio tribunal no choca consigo misma.
        using var propia = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/Causas/{editada.Id}", admin.Token, new
            {
                tribunal = "3° juzgado civil de Santiago",
                descripcion = "A / B"
            });
        Assert.Equal(HttpStatusCode.OK, propia.StatusCode);
    }

    private static async Task<string?> LeerMensajeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("mensaje").GetString();
}
