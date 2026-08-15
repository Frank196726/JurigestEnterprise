using System.Net;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class AuditoriaSeguridadTests
{
    [Fact]
    public async Task CrearUsuario_RegistraAuditoriaSinCredenciales()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        var admin =
            await SeguridadTestHelper.IniciarSesionAsync(
                client,
                SeguridadTestHelper.AdminEmail,
                SeguridadTestHelper.AdminPassword);

        var procuradorId =
            await SeguridadTestHelper.CrearProcuradorAsync(
                client,
                admin.Token);

        using var response =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/auditorias?cantidad=10",
                admin.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var contenido = await response.Content
            .ReadAsStringAsync();

        Assert.DoesNotContain(
            SeguridadTestHelper.ProcuradorPassword,
            contenido);

        Assert.DoesNotContain(
            "passwordHash",
            contenido);

        Assert.DoesNotContain(
            "accessToken",
            contenido);

        using var documento =
            JsonDocument.Parse(contenido);

        var raiz = documento.RootElement;
        var auditorias = raiz.GetProperty("value");

        Assert.Equal(
            1,
            raiz.GetProperty("count").GetInt32());

        var auditoria = auditorias[0];

        Assert.Equal(
            "UsuarioCreado",
            auditoria.GetProperty("accion").GetString());

        Assert.Equal(
            admin.UsuarioId,
            auditoria
                .GetProperty("usuarioActorId")
                .GetGuid());

        Assert.Equal(
            procuradorId,
            auditoria
                .GetProperty("usuarioAfectadoId")
                .GetGuid());

        Assert.Equal(
            "Rol asignado: Procurador.",
            auditoria.GetProperty("detalle").GetString());
    }

    [Fact]
    public async Task Procurador_NoPuedeConsultarAuditorias()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        var admin =
            await SeguridadTestHelper.IniciarSesionAsync(
                client,
                SeguridadTestHelper.AdminEmail,
                SeguridadTestHelper.AdminPassword);

        await SeguridadTestHelper.CrearProcuradorAsync(
            client,
            admin.Token);

        var procurador =
            await SeguridadTestHelper.IniciarSesionAsync(
                client,
                SeguridadTestHelper.ProcuradorEmail,
                SeguridadTestHelper.ProcuradorPassword);

        using var response =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/auditorias",
                procurador.Token);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}