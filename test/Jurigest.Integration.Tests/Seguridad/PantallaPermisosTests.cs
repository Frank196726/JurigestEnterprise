using System.Net;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Web.Security;
using Jurigest.Web.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class PantallaPermisosTests
{
    [Theory]
    [InlineData("valida")]
    [InlineData("vencida")]
    [InlineData("revocada")]
    [InlineData("sin-permiso")]
    [InlineData("api-caida")]
    public async Task PantallaDistingueSesionPermisosYDisponibilidad(string caso)
    {
        await using var api = new JurigestApiFactory();
        using var apiClient = api.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(apiClient);
        var login = await SeguridadTestHelper.IniciarSesionAsync(apiClient, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        await using var web = new WebApplicationFactory<SesionWeb>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHttpClientFactory>();
                services.AddSingleton<IHttpClientFactory>(new ApiFactory(api, caso));
            });
        });
        using var client = web.CreateClient(new() { AllowAutoRedirect = false });
        var store = web.Services.GetRequiredService<ISesionWebStore>();
        var id = store.Crear(new SesionWeb(caso == "revocada" ? "token-invalido" : login.Token, caso == "vencida" ? DateTime.UtcNow.AddMinutes(-5) : DateTime.UtcNow.AddHours(1), login.RefreshToken,
            DateTime.UtcNow.AddDays(1), login.UsuarioId, "Administrador", SeguridadTestHelper.AdminEmail, "Administrador", "Administrador", false));
        client.DefaultRequestHeaders.Add("Cookie", $"{SeguridadWebEndpoints.CookieName}={id}");
        using var response = await client.GetAsync("/permisos");
        var html = await response.Content.ReadAsStringAsync();
                if (caso is "vencida" or "revocada")
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("/login", response.Headers.Location!.OriginalString);
            return;
        }
        if (caso == "sin-permiso")
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/acceso-denegado", response.Headers.Location!.OriginalString);
            return;
        }
        if (caso == "api-caida")
        {
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Contains("No se pudieron comprobar tus permisos", html);
            Assert.DoesNotContain("Not Found", html);
            return;
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Permisos por rol", html);
        Assert.Contains("Guardar permisos", html);
        Assert.DoesNotContain("Not Found", html);
    }

    private sealed class RespuestaSimulada(string caso) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(
            new HttpResponseMessage(caso == "api-caida" ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)
            { Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json") });
    }
    private sealed class ApiFactory(JurigestApiFactory api, string caso) : IHttpClientFactory
    {
                public HttpClient CreateClient(string name) => caso is "sin-permiso" or "api-caida"
            ? new HttpClient(new RespuestaSimulada(caso)) { BaseAddress = new Uri("http://localhost") }
            : api.CreateClient();
    }
}

