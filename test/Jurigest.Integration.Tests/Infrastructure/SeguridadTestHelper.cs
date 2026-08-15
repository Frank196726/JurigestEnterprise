using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Infrastructure;

internal static class SeguridadTestHelper
{
    internal const string AdminEmail =
        "admin@jurigest.test";

    internal const string AdminPassword =
        "Admin.Test.2026!";

    internal const string ProcuradorEmail =
        "procurador@jurigest.test";

    internal const string ProcuradorPassword =
        "Procurador.Test.2026!";

    internal static async Task CrearAdministradorAsync(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/seguridad/bootstrap",
            new
            {
                nombre = "Administrador de pruebas",
                email = AdminEmail,
                password = AdminPassword
            });

        var detalle = await response.Content
    .ReadAsStringAsync();

    Assert.True(
        response.StatusCode == HttpStatusCode.Created,
        $"Bootstrap devolvio {(int)response.StatusCode}. " +
        $"Detalle: {detalle}");

    }

    internal static async Task<LoginResult> IniciarSesionAsync(
        HttpClient client,
        string email,
        string password)
    {
        var response = await client.PostAsJsonAsync(
            "/api/seguridad/login",
            new
            {
                email,
                password
            });

        response.EnsureSuccessStatusCode();

        var contenido = await response.Content
            .ReadAsStringAsync();

        using var documento =
            JsonDocument.Parse(contenido);

        var raiz = documento.RootElement;

        return new LoginResult(
            raiz.GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException(
                    "La respuesta no contiene accessToken."),
            raiz.GetProperty("usuarioId").GetGuid());
    }

    internal static async Task<Guid> CrearProcuradorAsync(
        HttpClient client,
        string adminToken)
    {
        using var response = await EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            "/api/seguridad/usuarios",
            adminToken,
            new
            {
                nombre = "Procurador de pruebas",
                email = ProcuradorEmail,
                password = ProcuradorPassword,
                rol = 3
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var contenido = await response.Content
            .ReadAsStringAsync();

        using var documento =
            JsonDocument.Parse(contenido);

        return documento.RootElement
            .GetProperty("id")
            .GetGuid();
    }

    internal static async Task<HttpResponseMessage>
        EnviarAutorizadoAsync(
            HttpClient client,
            HttpMethod method,
            string uri,
            string token,
            object? body = null)
    {
        var request = new HttpRequestMessage(
            method,
            uri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return await client.SendAsync(request);
    }
}

internal sealed record LoginResult(
    string Token,
    Guid UsuarioId);