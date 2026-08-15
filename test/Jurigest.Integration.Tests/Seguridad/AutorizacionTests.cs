using System.Net;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class AutorizacionTests
{
    [Fact]
    public async Task ObtenerCausas_SinToken_Devuelve401()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/Causas");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Procurador_LeeCausas_PeroNoPuedeCrearlas()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        var procurador = await PrepararProcuradorAsync(
            client);

        using var lectura =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/Causas",
                procurador.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            lectura.StatusCode);

        using var creacion =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Post,
                "/api/Causas",
                procurador.Token,
                new
                {
                    id = Guid.NewGuid(),
                    rit = "C-TEST-001-2026",
                    tribunal = "Tribunal de pruebas",
                    descripcion = "No debe crearse"
                });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            creacion.StatusCode);
    }

    [Fact]
    public async Task Procurador_NoPuedeAdministrarUsuarios()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        var procurador = await PrepararProcuradorAsync(
            client);

        using var response =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/usuarios",
                procurador.Token);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Administrador_NoPuedeDesactivarseASiMismo()
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

        using var response =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Put,
                $"/api/seguridad/usuarios/{admin.UsuarioId}/estado",
                admin.Token,
                new
                {
                    activo = false
                });

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task DesactivarUsuario_RevocaTokenExistente()
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

        var procurador =
            await SeguridadTestHelper.IniciarSesionAsync(
                client,
                SeguridadTestHelper.ProcuradorEmail,
                SeguridadTestHelper.ProcuradorPassword);

        using var antes =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/Causas",
                procurador.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            antes.StatusCode);

        using var desactivacion =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Put,
                $"/api/seguridad/usuarios/{procuradorId}/estado",
                admin.Token,
                new
                {
                    activo = false
                });

        Assert.Equal(
            HttpStatusCode.OK,
            desactivacion.StatusCode);

        using var despues =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/Causas",
                procurador.Token);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            despues.StatusCode);
        }
        [Fact]
        public async Task RestablecerPassword_RevocaTokenExistente()
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

        using var antes =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/Causas",
                procurador.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            antes.StatusCode);

        const string nuevaPassword =
            "Procurador.Nueva.2026!";

        using var restablecimiento =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Put,
                "/api/seguridad/usuarios/password",
                admin.Token,
                new
                {
                    email =
                        SeguridadTestHelper.ProcuradorEmail,
                    nuevaPassword
                });

        Assert.Equal(
            HttpStatusCode.OK,
            restablecimiento.StatusCode);

        using var despues =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/Causas",
                procurador.Token);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            despues.StatusCode);

        var nuevoLogin =
            await SeguridadTestHelper.IniciarSesionAsync(
                client,
                SeguridadTestHelper.ProcuradorEmail,
                nuevaPassword);

        using var conTokenNuevo =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/Causas",
                nuevoLogin.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            conTokenNuevo.StatusCode);
        }

    private static async Task<LoginResult>
        PrepararProcuradorAsync(HttpClient client)
    {
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

        return await SeguridadTestHelper.IniciarSesionAsync(
            client,
            SeguridadTestHelper.ProcuradorEmail,
            SeguridadTestHelper.ProcuradorPassword);
    }
}