using Jurigest.API.Security;
using Jurigest.Integration.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class IpClienteWebTests
{
    private const string IpNoConfiable = "203.0.113.50";

    [Fact]
    public async Task Login_DosUsuariosInformadosPorLaWeb_TienenLimitesSeparados()
    {
        await using var factory = new JurigestApiFactory();
        using var usuarioA = CrearCliente(factory, ConexionTestFilter.IpWebConfiable, "198.51.100.1");
        using var usuarioB = CrearCliente(factory, ConexionTestFilter.IpWebConfiable, "198.51.100.2");

        for (var intento = 1; intento <= 5; intento++)
            Assert.Equal(HttpStatusCode.Unauthorized, await LoginFallidoAsync(usuarioA));

        Assert.Equal(HttpStatusCode.TooManyRequests, await LoginFallidoAsync(usuarioA));
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginFallidoAsync(usuarioB));
    }

    [Fact]
    public async Task Login_CabeceraDesdeOrigenNoConfiable_SeIgnora()
    {
        await using var factory = new JurigestApiFactory();

        // Cambiar la IP informada en cada intento no evita el límite.
        for (var intento = 1; intento <= 5; intento++)
        {
            using var cliente = CrearCliente(factory, IpNoConfiable, $"198.51.100.{intento}");
            Assert.Equal(HttpStatusCode.Unauthorized, await LoginFallidoAsync(cliente));
        }

        using var falsificado = CrearCliente(factory, IpNoConfiable, "198.51.100.99");
        Assert.Equal(HttpStatusCode.TooManyRequests, await LoginFallidoAsync(falsificado));
    }

    [Theory]
    [InlineData(ConexionTestFilter.IpWebConfiable, "198.51.100.7", "198.51.100.7")]
    [InlineData(IpNoConfiable, "198.51.100.7", IpNoConfiable)]
    public async Task CrearUsuario_AuditoriaRegistraIpSoloSiLaInformaLaWeb(
        string ipConexion,
        string ipInformada,
        string ipEsperada)
    {
        await using var factory = new JurigestApiFactory();
        using var client = CrearCliente(factory, ipConexion, ipInformada);

        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        await SeguridadTestHelper.CrearProcuradorAsync(client, admin.Token);

        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/seguridad/auditorias?cantidad=10", admin.Token);
        response.EnsureSuccessStatusCode();

        using var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var auditoria = documento.RootElement.GetProperty("value").EnumerateArray()
            .Single(x => x.GetProperty("accion").GetString() == "UsuarioCreado");

        Assert.Equal(ipEsperada, auditoria.GetProperty("direccionIp").GetString());
    }

    private static HttpClient CrearCliente(
        JurigestApiFactory factory,
        string ipConexion,
        string ipInformada) =>
        factory.CreateDefaultClient(new CabecerasFijasHandler(new Dictionary<string, string>
        {
            [ConexionTestFilter.Cabecera] = ipConexion,
            [IpClienteWeb.Cabecera] = ipInformada
        }));

    private static async Task<HttpStatusCode> LoginFallidoAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/seguridad/login",
            new
            {
                email = "inexistente@jurigest.test",
                password = "Password.Incorrecta.2026!"
            });

        return response.StatusCode;
    }
}
