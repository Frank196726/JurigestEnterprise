using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class RecuperacionPasswordTests
{
    private const string NuevaPassword =
        "Admin.Nueva.Test.2026!";

    [Fact]
    public async Task Solicitud_NoRevelaSiElEmailExiste()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        using var existente =
            await client.PostAsJsonAsync(
                "/api/seguridad/password/recuperacion/solicitar",
                new
                {
                    email = SeguridadTestHelper.AdminEmail
                });

        using var inexistente =
            await client.PostAsJsonAsync(
                "/api/seguridad/password/recuperacion/solicitar",
                new
                {
                    email = "inexistente@jurigest.test"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            existente.StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            inexistente.StatusCode);

        var contenidoExistente =
            await existente.Content.ReadAsStringAsync();

        var contenidoInexistente =
            await inexistente.Content.ReadAsStringAsync();

        using var documentoExistente =
            JsonDocument.Parse(contenidoExistente);

        using var documentoInexistente =
            JsonDocument.Parse(contenidoInexistente);

        var mensajeExistente =
            documentoExistente.RootElement
                .GetProperty("mensaje")
                .GetString();

        var mensajeInexistente =
            documentoInexistente.RootElement
                .GetProperty("mensaje")
                .GetString();

        Assert.Equal(
            mensajeExistente,
            mensajeInexistente);

        Assert.False(
            documentoExistente.RootElement
                .TryGetProperty(
                    "tokenDesarrollo",
                    out _));

        Assert.False(
            documentoInexistente.RootElement
                .TryGetProperty(
                    "tokenDesarrollo",
                    out _));

        var notificacionExistente =
            factory.RecuperacionPasswordNotifier.ObtenerUltima(
                SeguridadTestHelper.AdminEmail);

        var notificacionInexistente =
            factory.RecuperacionPasswordNotifier.ObtenerUltima(
                "inexistente@jurigest.test");

        Assert.NotNull(notificacionExistente);
        Assert.Null(notificacionInexistente);
    }

    [Fact]
    public async Task Token_SeUsaUnaVezEInvalidaSesiones()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        var loginAnterior =
            await SeguridadTestHelper.IniciarSesionAsync(
                client,
                SeguridadTestHelper.AdminEmail,
                SeguridadTestHelper.AdminPassword);

        using var solicitud =
            await client.PostAsJsonAsync(
                "/api/seguridad/password/recuperacion/solicitar",
                new
                {
                    email = SeguridadTestHelper.AdminEmail
                });

        Assert.Equal(
            HttpStatusCode.OK,
            solicitud.StatusCode);

        var notificacion =
            factory.RecuperacionPasswordNotifier.ObtenerUltima(
                SeguridadTestHelper.AdminEmail);

        Assert.NotNull(notificacion);
        Assert.False(
            string.IsNullOrWhiteSpace(notificacion.Token));

        var token = notificacion.Token;

        using var confirmacion =
            await client.PostAsJsonAsync(
                "/api/seguridad/password/recuperacion/confirmar",
                new
                {
                    token,
                    nuevaPassword = NuevaPassword
                });

        Assert.Equal(
            HttpStatusCode.OK,
            confirmacion.StatusCode);

        using var reutilizacion =
            await client.PostAsJsonAsync(
                "/api/seguridad/password/recuperacion/confirmar",
                new
                {
                    token,
                    nuevaPassword = NuevaPassword
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            reutilizacion.StatusCode);

        using var sesionAnterior =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/sesiones",
                loginAnterior.Token);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            sesionAnterior.StatusCode);

        using var loginPasswordAnterior =
            await client.PostAsJsonAsync(
                "/api/seguridad/login",
                new
                {
                    email = SeguridadTestHelper.AdminEmail,
                    password =
                        SeguridadTestHelper.AdminPassword
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            loginPasswordAnterior.StatusCode);

        using var loginPasswordNueva =
            await client.PostAsJsonAsync(
                "/api/seguridad/login",
                new
                {
                    email = SeguridadTestHelper.AdminEmail,
                    password = NuevaPassword
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginPasswordNueva.StatusCode);

        using var scope =
            factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<JurigestDbContext>();

        var acciones = await context.AuditoriasSeguridad
            .AsNoTracking()
            .Select(auditoria => auditoria.Accion)
            .ToListAsync();

        Assert.Contains(
            "RecuperacionPasswordSolicitada",
            acciones);

        Assert.Contains(
            "PasswordRecuperada",
            acciones);
    }
}