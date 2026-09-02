using System.Net;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class ResolucionesTests
{
    [Fact]
    public async Task Administrador_RegistraConsultaYEliminaResolucion()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var crearCausa = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, "/api/Causas", admin.Token, new
        {
            id = Guid.Empty,
            rit = "C-RES-001-2026",
            tribunal = "Tribunal de integración",
            descripcion = "Prueba de resoluciones",
            fechaEncargoCausa = DateTime.UtcNow.Date
        });
        crearCausa.EnsureSuccessStatusCode();
        var causaId = await LeerGuidAsync(crearCausa, "id");

        using var registrar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post, $"/api/Causas/{causaId}/resoluciones", admin.Token, new
        {
            tipo = 4,
            fecha = new DateTime(2026, 9, 2),
            descripcion = "Sentencia definitiva de integración"
        });
        Assert.Equal(HttpStatusCode.Created, registrar.StatusCode);
        var resolucionId = await LeerGuidAsync(registrar, "id");

        using var listar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, $"/api/Causas/{causaId}/resoluciones", admin.Token);
        listar.EnsureSuccessStatusCode();
        using var listado = JsonDocument.Parse(await listar.Content.ReadAsStringAsync());
        Assert.Contains(listado.RootElement.EnumerateArray(), item => item.GetProperty("id").GetGuid() == resolucionId && item.GetProperty("descripcion").GetString() == "Sentencia definitiva de integración");

        using var eliminar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Delete, $"/api/Resoluciones/{resolucionId}", admin.Token);
        Assert.Equal(HttpStatusCode.OK, eliminar.StatusCode);

        using var obtenerEliminada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, $"/api/Resoluciones/{resolucionId}", admin.Token);
        Assert.Equal(HttpStatusCode.NotFound, obtenerEliminada.StatusCode);
    }

    private static async Task<Guid> LeerGuidAsync(HttpResponseMessage response, string propiedad)
    {
        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return documento.RootElement.GetProperty(propiedad).GetGuid();
    }
}
