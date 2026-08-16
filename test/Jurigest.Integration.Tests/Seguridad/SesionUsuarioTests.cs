using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class SesionUsuarioTests
{
    [Fact]
    public async Task RefreshYLogout_RotanYRevocanRefreshTokens()
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

        Assert.False(
            string.IsNullOrWhiteSpace(login.RefreshToken));

        using var refreshResponse =
            await client.PostAsJsonAsync(
                "/api/seguridad/refresh",
                new
                {
                    refreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.OK,
            refreshResponse.StatusCode);

        var refreshContenido =
            await refreshResponse.Content
                .ReadAsStringAsync();

        using var refreshDocumento =
            JsonDocument.Parse(refreshContenido);

        var refreshTokenNuevo =
            refreshDocumento.RootElement
                .GetProperty("refreshToken")
                .GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(refreshTokenNuevo));

        Assert.NotEqual(
            login.RefreshToken,
            refreshTokenNuevo);

        using var reutilizacionAnterior =
            await client.PostAsJsonAsync(
                "/api/seguridad/refresh",
                new
                {
                    refreshToken = login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            reutilizacionAnterior.StatusCode);

        using var logoutResponse =
            await client.PostAsJsonAsync(
                "/api/seguridad/logout",
                new
                {
                    refreshToken = refreshTokenNuevo
                });

        Assert.Equal(
            HttpStatusCode.OK,
            logoutResponse.StatusCode);

        using var refreshDespuesDeLogout =
            await client.PostAsJsonAsync(
                "/api/seguridad/refresh",
                new
                {
                    refreshToken = refreshTokenNuevo
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refreshDespuesDeLogout.StatusCode);

        using var segundoLogout =
            await client.PostAsJsonAsync(
                "/api/seguridad/logout",
                new
                {
                    refreshToken = refreshTokenNuevo
                });

        Assert.Equal(
            HttpStatusCode.OK,
            segundoLogout.StatusCode);
    }
}