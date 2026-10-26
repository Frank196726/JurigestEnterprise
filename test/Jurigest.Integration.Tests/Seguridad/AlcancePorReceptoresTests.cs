using Jurigest.Integration.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class AlcancePorReceptoresTests
{
    private const string Email = "multireceptor@jurigest.test";
    private const string Password = "MultiReceptor.Test.2026!";

    [Fact]
    public async Task Usuario_VeSoloCausasDeSusReceptores_YSePuedeAsignarANuevosReceptores()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        var receptorUno = await SeguridadTestHelper.CrearReceptorAsync(client, admin.Token, "Receptor Uno");
        var receptorDos = await SeguridadTestHelper.CrearReceptorAsync(client, admin.Token, "Receptor Dos");
        var receptorTres = await SeguridadTestHelper.CrearReceptorAsync(client, admin.Token, "Receptor Tres");

        var causaUno = await CrearCausaAsync(client, admin.Token, "C-MUL-001-2026");
        var causaDos = await CrearCausaAsync(client, admin.Token, "C-MUL-002-2026");
        var causaTres = await CrearCausaAsync(client, admin.Token, "C-MUL-003-2026");
        var causaSinReceptor = await CrearCausaAsync(client, admin.Token, "C-MUL-004-2026");
        var diligenciaUno = await AgregarDiligenciaAsync(client, admin.Token, causaUno, "Receptor Uno");
        await AgregarDiligenciaAsync(client, admin.Token, causaDos, "Receptor Dos");
        var diligenciaTres = await AgregarDiligenciaAsync(client, admin.Token, causaTres, "Receptor Tres");
        // Causa compartida: su primera diligencia es de un receptor ajeno y la segunda, de uno propio.
        var causaCompartida = await CrearCausaAsync(client, admin.Token, "C-MUL-005-2026");
        await AgregarDiligenciaAsync(client, admin.Token, causaCompartida, "Receptor Tres");
        await AgregarDiligenciaAsync(client, admin.Token, causaCompartida, "Receptor Uno");

        // Sin receptores asociados no se puede crear el usuario.
        using var sinReceptor = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/seguridad/usuarios", admin.Token, DatosUsuario([]));
        Assert.Equal(HttpStatusCode.BadRequest, sinReceptor.StatusCode);

        using var creado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/seguridad/usuarios", admin.Token,
            DatosUsuario([receptorUno, receptorDos]));
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var usuarioId = (await creado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        using var detalle = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/seguridad/usuarios/{usuarioId}", admin.Token);
        var asignados = (await detalle.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("receptores");
        Assert.Equal(2, asignados.GetArrayLength());

        var usuario = await SeguridadTestHelper.IniciarSesionAsync(client, Email, Password);

        // Ve las causas de ambos receptores y la que aún no tiene receptor.
        Assert.Equal(
            new[] { causaUno, causaDos, causaSinReceptor, causaCompartida }.Order(),
            (await IdsCausasAsync(client, usuario.Token)).Order());

        using var causaAjena = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/Causas/{causaTres}", usuario.Token);
        Assert.Equal(HttpStatusCode.NotFound, causaAjena.StatusCode);
        using var diligenciaAjena = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/Diligencias/{diligenciaTres}", usuario.Token);
        Assert.Equal(HttpStatusCode.NotFound, diligenciaAjena.StatusCode);
        using var diligenciasAjenas = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/Diligencias/causa/{causaTres}", usuario.Token);
        Assert.Equal(HttpStatusCode.NotFound, diligenciasAjenas.StatusCode);

        // El catálogo solo ofrece sus receptores y no puede asignar uno ajeno.
        using var catalogo = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Catalogos/receptores", usuario.Token);
        Assert.Equal(2, (await catalogo.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());
        using var asignarAjeno = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaUno}/asignar-receptor", usuario.Token,
            new { receptorJudicial = "Receptor Tres" });
        Assert.Equal(HttpStatusCode.BadRequest, asignarAjeno.StatusCode);
        using var asignarPropio = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaUno}/asignar-receptor", usuario.Token,
            new { receptorJudicial = "Receptor Dos" });
        Assert.Equal(HttpStatusCode.OK, asignarPropio.StatusCode);

        // Editar una causa visible reenviando su receptor actual (ajeno) no es una asignación.
        using var edicionSinCambio = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Causas/{causaCompartida}", usuario.Token,
            DatosCausa("Receptor Tres"));
        Assert.DoesNotContain("Solo puede asignar", await edicionSinCambio.Content.ReadAsStringAsync());
        using var edicionAjena = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Causas/{causaCompartida}", usuario.Token,
            DatosCausa("Receptor Ajeno"));
        Assert.Equal(HttpStatusCode.BadRequest, edicionAjena.StatusCode);

        // Un receptor que crea el propio usuario queda asociado a él y puede asignarlo.
        using var receptorNuevo = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/Catalogos/receptores", usuario.Token,
            new { nombre = "Receptor Nuevo" });
        Assert.Equal(HttpStatusCode.OK, receptorNuevo.StatusCode);
        var receptorNuevoId = (await receptorNuevo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var catalogoAmpliado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Catalogos/receptores", usuario.Token);
        Assert.Equal(3, (await catalogoAmpliado.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());
        using var asignarNuevo = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaUno}/asignar-receptor", usuario.Token,
            new { receptorJudicial = "Receptor Nuevo" });
        Assert.Equal(HttpStatusCode.OK, asignarNuevo.StatusCode);

        // Al asignarle un receptor adicional, pasa a ver también sus causas.
        using var editado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/seguridad/usuarios/{usuarioId}", admin.Token,
            DatosUsuario([receptorUno, receptorDos, receptorTres, receptorNuevoId]));
        Assert.Equal(HttpStatusCode.OK, editado.StatusCode);

        Assert.Equal(5, (await IdsCausasAsync(client, usuario.Token)).Count);
        using var causaAhoraPropia = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/Causas/{causaTres}", usuario.Token);
        Assert.Equal(HttpStatusCode.OK, causaAhoraPropia.StatusCode);

        // El administrador no tiene restricción.
        Assert.Equal(5, (await IdsCausasAsync(client, admin.Token)).Count);

        // Un cliente que no envía receptores conserva los actuales.
        using var sinCambioReceptores = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/seguridad/usuarios/{usuarioId}", admin.Token,
            new { nombre = "Usuario Multireceptor", email = Email, rut = "15.003.942-8", telefono = "+56 9 5555 0505", direccion = "Calle Receptores 50", numeroOficina = "5" });
        Assert.Equal(HttpStatusCode.OK, sinCambioReceptores.StatusCode);
        using var detalleFinal = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, $"/api/seguridad/usuarios/{usuarioId}", admin.Token);
        Assert.Equal(4, (await detalleFinal.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("receptores").GetArrayLength());
    }

    private static object DatosCausa(string receptorJudicial) => new
    {
        tribunal = "1° Juzgado Civil de Santiago",
        descripcion = "Demandante / Demandado",
        fechaEncargoCausa = DateTime.UtcNow.Date,
        receptorJudicial
    };

    private static object DatosUsuario(Guid[] receptorIds) => new
    {
        nombre = "Usuario Multireceptor",
        email = Email,
        password = Password,
        rol = 2,
        rut = "15.003.942-8",
        telefono = "+56 9 5555 0505",
        direccion = "Calle Receptores 50",
        numeroOficina = "5",
        receptorIds
    };

    private static async Task<List<Guid>> IdsCausasAsync(HttpClient client, string token)
    {
        using var respuesta = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas", token);
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<JsonElement>())
            .EnumerateArray()
            .Select(x => x.GetProperty("id").GetGuid())
            .ToList();
    }

    private static async Task<Guid> CrearCausaAsync(HttpClient client, string token, string rit)
    {
        using var response = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/Causas", token,
            new
            {
                id = Guid.Empty,
                rit,
                tribunal = "1° Juzgado Civil de Santiago",
                descripcion = "Demandante / Demandado",
                fechaEncargoCausa = DateTime.UtcNow.Date
            });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> AgregarDiligenciaAsync(
        HttpClient client, string token, Guid causaId, string receptor)
    {
        using var crear = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, $"/api/Causas/{causaId}/diligencias", token,
            new { descripcion = "Notificación", tipo = 1 });
        crear.EnsureSuccessStatusCode();
        var diligenciaId = (await crear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        using var asignar = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/Diligencias/{diligenciaId}/asignar-receptor", token,
            new { receptorJudicial = receptor });
        asignar.EnsureSuccessStatusCode();
        return diligenciaId;
    }
}
