using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class RolPerfilYMensajesTests
{
    [Fact]
    public async Task CambiarPerfilDeRol_ActualizaUsuarios_RestableceSusPermisos_YCierraSesiones()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);

        using var crearRol = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/seguridad/roles", admin.Token, new { nombre = "Operador", perfil = 3 });
        crearRol.EnsureSuccessStatusCode();
        var rolId = (await crearRol.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var permisos = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/seguridad/permisos/{rolId}", admin.Token,
            new { seleccion = new[] { "CausasLectura", "DiligenciasLectura" }, version = Guid.Empty });
        permisos.EnsureSuccessStatusCode();

        var receptorId = await SeguridadTestHelper.CrearReceptorAsync(client, admin.Token);
        using var creado = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Post, "/api/seguridad/usuarios", admin.Token,
            new
            {
                nombre = "Usuario Operador",
                email = "operador@jurigest.test",
                password = "Operador.Test.2026!",
                rol = 3,
                rolCatalogoId = rolId,
                rut = "15.003.942-8",
                telefono = "+56 9 5555 0707",
                direccion = "Calle Operador 70",
                numeroOficina = "7",
                receptorIds = new[] { receptorId }
            });
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        var usuario = await SeguridadTestHelper.IniciarSesionAsync(client, "operador@jurigest.test", "Operador.Test.2026!");

        using var administrador = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/seguridad/roles/{rolId}/perfil", admin.Token, new { perfil = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, administrador.StatusCode);

        using var cambio = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/seguridad/roles/{rolId}/perfil", admin.Token, new { perfil = 2 });
        Assert.Equal(HttpStatusCode.OK, cambio.StatusCode);
        var resultado = await cambio.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, resultado.GetProperty("perfil").GetInt32());
        Assert.Equal(1, resultado.GetProperty("usuariosAfectados").GetInt32());
        Assert.True(resultado.GetProperty("permisosRestablecidos").GetBoolean());

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JurigestDbContext>();
            var guardado = await db.Usuarios.AsNoTracking().SingleAsync(x => x.Email == "operador@jurigest.test");
            Assert.Equal(RolUsuario.Abogado, guardado.Rol);
            Assert.Equal(rolId, guardado.RolCatalogoId);
            Assert.Equal("Operador", guardado.NombreRolPersonalizado);
            Assert.False(await db.Set<PermisosRol>().AnyAsync(x => x.Clave == rolId.ToString()));
        }

        // La sesión abierta con el perfil anterior deja de ser válida.
        using var sesionAnterior = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/Causas", usuario.Token);
        Assert.Equal(HttpStatusCode.Unauthorized, sesionAnterior.StatusCode);

        // Con el nuevo perfil (Abogado) tiene los permisos predeterminados, incluida la escritura de causas.
        // El primer ingreso cambió la contraseña temporal agregando ".Nueva" (ver SeguridadTestHelper).
        var nuevaSesion = await SeguridadTestHelper.IniciarSesionAsync(client, "operador@jurigest.test", "Operador.Test.2026!.Nueva");
        using var mios = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Get, "/api/seguridad/permisos/mios", nuevaSesion.Token);
        Assert.Contains("CausasEscritura", (await mios.Content.ReadFromJsonAsync<string[]>())!);
    }

    [Fact]
    public async Task RutInvalido_DevuelveMensajeSinDetalleTecnico()
    {
        await using var factory = new JurigestApiFactory();
        using var client = factory.CreateClient();
        await SeguridadTestHelper.CrearAdministradorAsync(client);
        var admin = await SeguridadTestHelper.IniciarSesionAsync(
            client, SeguridadTestHelper.AdminEmail, SeguridadTestHelper.AdminPassword);
        var id = await SeguridadTestHelper.CrearProcuradorAsync(client, admin.Token);

        using var respuesta = await SeguridadTestHelper.EnviarAutorizadoAsync(
            client, HttpMethod.Put, $"/api/seguridad/usuarios/{id}", admin.Token,
            new { nombre = "Procurador de pruebas", email = SeguridadTestHelper.ProcuradorEmail, rut = "12345678-9", telefono = "123456789", direccion = "Huerfanos 12345", numeroOficina = "405" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("El dígito verificador del RUT no es válido.",
            (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("mensaje").GetString());
    }
}
