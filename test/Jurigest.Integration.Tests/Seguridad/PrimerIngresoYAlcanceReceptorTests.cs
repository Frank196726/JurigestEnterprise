using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class PrimerIngresoYAlcanceReceptorTests
{
    [Fact]
    public async Task Receptor_CambiaClaveInicial_MuestraRolAsignado_YVeSoloSusCausas()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client,
            SeguridadTestHelper.AdminEmail,
            SeguridadTestHelper.AdminPassword);

        using var rolResponse = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            "/api/seguridad/roles",
            admin.Token,
            new { nombre = "Receptor", perfil = 3 });
        rolResponse.EnsureSuccessStatusCode();
        var rol = await rolResponse.Content.ReadFromJsonAsync<JsonElement>();

        using var usuarioResponse = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            "/api/seguridad/usuarios",
            admin.Token,
            new
            {
                nombre = "Marcela Vargas",
                rut = "15.003.942-8",
                telefono = "+56 9 5555 0404",
                direccion = "Avenida Jurídica 100",
                numeroOficina = "Oficina 14",
                email = "marcela@jurigest.test",
                password = "Temporal.Marcela.2026!",
                rol = 3,
                rolCatalogoId = rol.GetProperty("id").GetGuid()
            });
        Assert.Equal(HttpStatusCode.Created, usuarioResponse.StatusCode);

        var loginInicial = await LoginRawAsync(client, "marcela@jurigest.test", "Temporal.Marcela.2026!");
        Assert.True(loginInicial.Json.GetProperty("debeCambiarPassword").GetBoolean());
        Assert.Equal("Receptor", loginInicial.Json.GetProperty("rolAsignado").GetString());
        Assert.Equal("Procurador", loginInicial.Json.GetProperty("rol").GetString());

        using var bloqueado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas", loginInicial.Token);
        Assert.Equal(HttpStatusCode.Forbidden, bloqueado.StatusCode);

        using var cambio = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            "/api/seguridad/password/cambio-inicial",
            loginInicial.Token,
            new { nuevaPassword = "Marcela.Definitiva.2026!" });
        Assert.Equal(HttpStatusCode.OK, cambio.StatusCode);

        var receptor = await SeguridadTestHelper.IniciarSesionAsync(
            client, "marcela@jurigest.test", "Marcela.Definitiva.2026!");

        var causaAsignada = await CrearCausaAsync(client, admin.Token, "C-REC-001-2026");
        var causaAjena = await CrearCausaAsync(client, admin.Token, "C-REC-002-2026");
        await AgregarDiligenciaAsync(client, admin.Token, causaAsignada, "Marcela Vargas");
        await AgregarDiligenciaAsync(client, admin.Token, causaAjena, "Otro Receptor");

        using var causas = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas", receptor.Token);
        causas.EnsureSuccessStatusCode();
        var listado = await causas.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(listado.EnumerateArray());
        Assert.Equal(causaAsignada, listado.EnumerateArray().Single().GetProperty("id").GetGuid());

        using var ajena = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas/rit/C-REC-002-2026", receptor.Token);
        Assert.Equal(HttpStatusCode.NotFound, ajena.StatusCode);

        using var panel = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas/panel-ejecutivo", receptor.Token);
        panel.EnsureSuccessStatusCode();
        var resumen = await panel.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, resumen.GetProperty("totalCausas").GetInt32());

        // Al conceder creación al Receptor, Nueva gestión no envía un receptor en el formulario.
        // El servidor debe asignar el de la sesión, incluso si el cliente intenta enviar otro.
        using var permisos = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/seguridad/permisos/{rol.GetProperty("id").GetGuid()}", admin.Token,
            new { seleccion = new[] { "CausasLectura", "CausasEscritura", "DiligenciasLectura", "DiligenciasGestion" }, version = Guid.Empty });
        permisos.EnsureSuccessStatusCode();
        var causaPropia = await CrearCausaAsync(client, receptor.Token, "C-REC-003-2026");
        using var primeraDiligencia = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Post,
            $"/api/Causas/{causaPropia}/diligencias", receptor.Token,
            new { descripcion = "Embargo Vehículo", tipo = 3, receptorJudicial = "Otro Receptor" });
        primeraDiligencia.EnsureSuccessStatusCode();
        using var encontrada = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            "/api/Causas/rit/C-REC-003-2026", receptor.Token);
        Assert.Equal(HttpStatusCode.OK, encontrada.StatusCode);
        using var detalle = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            $"/api/Causas/{causaPropia}", receptor.Token);
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
        using var ultima = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get,
            $"/api/Diligencias/causa/{causaPropia}/ultima", receptor.Token);
        ultima.EnsureSuccessStatusCode();
        Assert.Equal("Marcela Vargas", (await ultima.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("receptorJudicial").GetString());
        using var listadoFinal = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/Causas", receptor.Token);
        Assert.Equal(2, (await listadoFinal.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());
        using var panelFinal = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/Causas/panel-ejecutivo", receptor.Token);
        Assert.Equal(2, (await panelFinal.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("totalCausas").GetInt32());
        using var editar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Put,
            $"/api/seguridad/usuarios/{receptor.UsuarioId}", admin.Token,
            new { nombre = "Marcela Alejandra Vargas Rojas", email = "marcela@jurigest.test", rut = "15.003.942-8", telefono = "999999999", direccion = "Avenida Jurídica 100", numeroOficina = "14" });
        Assert.Equal(HttpStatusCode.OK, editar.StatusCode);
        receptor = await SeguridadTestHelper.IniciarSesionAsync(client, "marcela@jurigest.test", "Marcela.Definitiva.2026!");
        using var trasRenombrar = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, "/api/Causas", receptor.Token);
        Assert.Equal(2, (await trasRenombrar.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());
        using var detalleRenombrado = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, $"/api/Causas/{causaAsignada}", receptor.Token);
        Assert.Equal(HttpStatusCode.OK, detalleRenombrado.StatusCode);
        using var ajenaFinal = await SeguridadTestHelper.EnviarAutorizadoAsync(client, HttpMethod.Get, $"/api/Causas/{causaAjena}", receptor.Token);
        Assert.Equal(HttpStatusCode.NotFound, ajenaFinal.StatusCode);
    }

    private static async Task<(string Token, JsonElement Json)> LoginRawAsync(
        HttpClient client,
        string email,
        string password)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/seguridad/login",
            new { email, password });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (json.GetProperty("accessToken").GetString()!, json.Clone());
    }

    private static async Task<Guid> CrearCausaAsync(
        HttpClient client,
        string token,
        string rit)
    {
        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            "/api/Causas",
            token,
            new
            {
                id = Guid.Empty,
                rit,
                tribunal = "1° Juzgado Civil de Santiago",
                descripcion = "Demandante / Demandado",
                fechaEncargoCausa = DateTime.UtcNow.Date
            });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }

    private static async Task AgregarDiligenciaAsync(
        HttpClient client,
        string token,
        Guid causaId,
        string receptor)
    {
        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Post,
            $"/api/Causas/{causaId}/diligencias",
            token,
            new { descripcion = "Notificación", tipo = 1 });
        crear.EnsureSuccessStatusCode();
        var json = await crear.Content.ReadFromJsonAsync<JsonElement>();
        var diligenciaId = json.GetProperty("id").GetGuid();

        using var asignar = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client,
            HttpMethod.Put,
            $"/api/Diligencias/{diligenciaId}/asignar-receptor",
            token,
            new { receptorJudicial = receptor });
        asignar.EnsureSuccessStatusCode();
    }
}
