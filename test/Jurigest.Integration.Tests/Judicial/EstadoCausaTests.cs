using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class EstadoCausaTests
{
    private static readonly DateTime FechaEncargo =
        new(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CambiarEstado_SinAutenticacion_Devuelve401()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PutAsJsonAsync(
            $"/api/Causas/{Guid.NewGuid()}/estado", new { estado = 3 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Administrador_CambiaEstadoManualmente()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        var token = await IniciarAdministradorAsync(client);
        var causaId = await CrearCausaAsync(client, token, "C-ESTADO-001-2026");

        Assert.Equal((int)EstadoCausa.Ingresada, await ObtenerEstadoAsync(client, token, causaId));

        foreach (var estado in new[] { EstadoCausa.Suspendida, EstadoCausa.Terminada, EstadoCausa.Archivada, EstadoCausa.EnTramitacion })
        {
            using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
                client, HttpMethod.Put, $"/api/Causas/{causaId}/estado", token, new { estado = (int)estado });

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal((int)estado, await ObtenerEstadoAsync(client, token, causaId));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CambiarEstado_ValorInvalido_Devuelve400(int estado)
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        var token = await IniciarAdministradorAsync(client);
        var causaId = await CrearCausaAsync(client, token, "C-ESTADO-002-2026");

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Causas/{causaId}/estado", token, new { estado });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CambiarEstado_CausaInexistente_Devuelve404()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        var token = await IniciarAdministradorAsync(client);

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Causas/{Guid.NewGuid()}/estado", token, new { estado = 3 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarResultado_PasaATramitacionYNoPermiteVolverAIngresada()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        var token = await IniciarAdministradorAsync(client);
        var causaId = await CrearCausaAsync(client, token, "C-ESTADO-003-2026");

        var diligenciaRealizadaId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.DiligenciasRealizadas.Add(new DiligenciaRealizadaCatalogo(
                diligenciaRealizadaId, "Notificación personal", (int)TipoDiligencia.Notificacion));
            await db.SaveChangesAsync();
        }

        using var crearDiligencia = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", token,
            new { descripcion = "Notificación de demanda", tipo = (int)TipoDiligencia.Notificacion });
        Assert.Equal(HttpStatusCode.OK, crearDiligencia.StatusCode);
        var diligenciaId = await LeerIdAsync(crearDiligencia);

        using var resultado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaId}/resultado", token,
            new
            {
                diligenciaRealizadaId,
                resultado = 1,
                resultadoDetalle = "Notificación realizada",
                estampe = "ESTAMPE",
                fechaGestion = FechaEncargo.AddDays(2)
            });
        Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);

        Assert.Equal((int)EstadoCausa.EnTramitacion, await ObtenerEstadoAsync(client, token, causaId));

        using var volver = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Causas/{causaId}/estado", token,
            new { estado = (int)EstadoCausa.Ingresada });

        Assert.Equal(HttpStatusCode.Conflict, volver.StatusCode);
        Assert.Contains("no puede volver al estado Ingresada", await volver.Content.ReadAsStringAsync());
        Assert.Equal((int)EstadoCausa.EnTramitacion, await ObtenerEstadoAsync(client, token, causaId));
    }

    [Theory]
    [InlineData(EstadoCausa.Suspendida, "suspendida")]
    [InlineData(EstadoCausa.Terminada, "terminada")]
    [InlineData(EstadoCausa.Archivada, "archivada")]
    public async Task CausaCerrada_RechazaDiligenciasYResultados(EstadoCausa estado, string nombre)
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        var token = await IniciarAdministradorAsync(client);
        var causaId = await CrearCausaAsync(client, token, $"C-ESTADO-1{(int)estado}-2026");

        var diligenciaRealizadaId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            db.DiligenciasRealizadas.Add(new DiligenciaRealizadaCatalogo(
                diligenciaRealizadaId, "Notificación personal", (int)TipoDiligencia.Notificacion));
            await db.SaveChangesAsync();
        }

        // Diligencia creada antes de cerrar la causa.
        using var previa = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", token,
            new { descripcion = "Notificación de demanda", tipo = (int)TipoDiligencia.Notificacion });
        Assert.Equal(HttpStatusCode.OK, previa.StatusCode);
        var diligenciaId = await LeerIdAsync(previa);

        using var cambiar = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Causas/{causaId}/estado", token, new { estado = (int)estado });
        Assert.Equal(HttpStatusCode.NoContent, cambiar.StatusCode);

        using var nueva = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", token,
            new { descripcion = "Embargo", tipo = (int)TipoDiligencia.Embargo });
        Assert.Equal(HttpStatusCode.Conflict, nueva.StatusCode);
        Assert.Contains($"La causa está {nombre}", await nueva.Content.ReadAsStringAsync());

        using var resultado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaId}/resultado", token,
            new
            {
                diligenciaRealizadaId,
                resultado = 1,
                resultadoDetalle = "Notificación realizada",
                estampe = "ESTAMPE",
                fechaGestion = FechaEncargo.AddDays(2)
            });
        Assert.Equal(HttpStatusCode.Conflict, resultado.StatusCode);
        Assert.Contains($"La causa está {nombre}", await resultado.Content.ReadAsStringAsync());

        await using (var verificacion = factory.Services.CreateAsyncScope())
        {
            var db = verificacion.ServiceProvider.GetRequiredService<JurigestDbContext>();
            Assert.Single(db.Diligencias.Where(d => d.CausaId == causaId).ToList());
            Assert.Empty(db.Recibos.Where(r => r.CausaId == causaId).ToList());
        }

        // Al reactivarla vuelve a aceptar diligencias.
        using var reactivar = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Causas/{causaId}/estado", token,
            new { estado = (int)EstadoCausa.EnTramitacion });
        Assert.Equal(HttpStatusCode.NoContent, reactivar.StatusCode);

        using var otraVez = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", token,
            new { descripcion = "Embargo", tipo = (int)TipoDiligencia.Embargo });
        Assert.Equal(HttpStatusCode.OK, otraVez.StatusCode);
    }

    private static async Task<string> IniciarAdministradorAsync(HttpClient client)
    {
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        return admin.Token;
    }

    private static async Task<Guid> CrearCausaAsync(HttpClient client, string token, string rit)
    {
        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/Causas", token,
            new
            {
                id = Guid.Empty,
                rit,
                tribunal = "Tribunal de integración",
                descripcion = "Prueba estado de causa",
                fechaEncargoCausa = FechaEncargo
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await LeerIdAsync(response);
    }

    private static async Task<int> ObtenerEstadoAsync(HttpClient client, string token, Guid causaId)
    {
        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/Causas/{causaId}", token);
        response.EnsureSuccessStatusCode();
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return documento.RootElement.GetProperty("estado").GetInt32();
    }

    private static async Task<Guid> LeerIdAsync(HttpResponseMessage response)
    {
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return documento.RootElement.GetProperty("id").GetGuid();
    }
}
