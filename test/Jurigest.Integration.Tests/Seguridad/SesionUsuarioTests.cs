using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class SesionUsuarioTests
{
    [Fact]
    public async Task Logout_RevocaRefreshTokenYEsIdempotente()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        var login = await SeguridadTestHelper
            .IniciarSesionAsync(
                client,
                SeguridadTestHelper.AdminEmail,
                SeguridadTestHelper.AdminPassword);

        using var logoutResponse =
            await client.PostAsJsonAsync(
                "/api/seguridad/logout",
                new
                {
                    refreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.OK,
            logoutResponse.StatusCode);

        using var refreshDespuesDeLogout =
            await client.PostAsJsonAsync(
                "/api/seguridad/refresh",
                new
                {
                    refreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshDespuesDeLogout.StatusCode);

        using var segundoLogout =
            await client.PostAsJsonAsync(
                "/api/seguridad/logout",
                new
                {
                    refreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.OK,
            segundoLogout.StatusCode);
    }

    [Fact]
    public async Task ReutilizarRefreshTokenAnterior_InvalidaTodasLasSesiones()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        var login = await SeguridadTestHelper
            .IniciarSesionAsync(
                client,
                SeguridadTestHelper.AdminEmail,
                SeguridadTestHelper.AdminPassword);

        using var renovacion =
            await client.PostAsJsonAsync(
                "/api/seguridad/refresh",
                new
                {
                    refreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.OK,
            renovacion.StatusCode);

        var contenido =
            await renovacion.Content.ReadAsStringAsync();

        using var documento =
            JsonDocument.Parse(contenido);

        var refreshTokenNuevo =
            documento.RootElement
                .GetProperty("refreshToken")
                .GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(refreshTokenNuevo));

        using var reutilizacion =
            await client.PostAsJsonAsync(
                "/api/seguridad/refresh",
                new
                {
                    refreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            reutilizacion.StatusCode);

        using var renovacionConTokenNuevo =
            await client.PostAsJsonAsync(
                "/api/seguridad/refresh",
                new
                {
                    refreshToken = refreshTokenNuevo
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            renovacionConTokenNuevo.StatusCode);

        using var accesoConJwtAnterior =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/Causas",
                login.Token);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            accesoConJwtAnterior.StatusCode);

        using var scope =
            factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<JurigestDbContext>();

        var auditoria =
            await context.AuditoriasSeguridad
                .AsNoTracking()
                .SingleAsync(auditoria =>
                    auditoria.Accion ==
                    "RefreshTokenReutilizado");

        Assert.Equal(
            login.UsuarioId,
            auditoria.UsuarioAfectadoId);

        Assert.Null(auditoria.UsuarioActorId);
    }
}