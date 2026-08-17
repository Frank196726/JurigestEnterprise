using System.Net;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class GestionSesionesTests
{
    [Fact]
    public async Task Usuario_PuedeListarYCerrarSusSesiones()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        var loginA = await IniciarSesionAsync(client);
        var loginB = await IniciarSesionAsync(client);
        var loginC = await IniciarSesionAsync(client);

        using var listadoA =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/sesiones",
                loginA.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            listadoA.StatusCode);

        var contenidoA =
            await listadoA.Content.ReadAsStringAsync();

        using var documentoA =
            JsonDocument.Parse(contenidoA);

        Assert.Equal(
            3,
            documentoA.RootElement
                .GetProperty("count")
                .GetInt32());

        Assert.DoesNotContain(
            "refreshtoken",
            contenidoA.ToLowerInvariant());

        Assert.DoesNotContain(
            "versionseguridad",
            contenidoA.ToLowerInvariant());

        using var listadoB =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/sesiones",
                loginB.Token);

        var contenidoB =
            await listadoB.Content.ReadAsStringAsync();

        var sesionBId =
            ObtenerSesionActualId(contenidoB);

        using var cerrarB =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Delete,
                $"/api/seguridad/sesiones/{sesionBId}",
                loginA.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            cerrarB.StatusCode);

        using var accesoB =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/sesiones",
                loginB.Token);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            accesoB.StatusCode);

        using var cerrarOtras =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Delete,
                "/api/seguridad/sesiones/otras",
                loginA.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            cerrarOtras.StatusCode);

        var contenidoCerrarOtras =
            await cerrarOtras.Content.ReadAsStringAsync();

        using var documentoCerrarOtras =
            JsonDocument.Parse(contenidoCerrarOtras);

        Assert.Equal(
            1,
            documentoCerrarOtras.RootElement
                .GetProperty("cantidad")
                .GetInt32());

        using var accesoC =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/sesiones",
                loginC.Token);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            accesoC.StatusCode);

        using var listadoFinal =
            await SeguridadTestHelper.EnviarAutorizadoAsync(
                client,
                HttpMethod.Get,
                "/api/seguridad/sesiones",
                loginA.Token);

        Assert.Equal(
            HttpStatusCode.OK,
            listadoFinal.StatusCode);

        var contenidoFinal =
            await listadoFinal.Content.ReadAsStringAsync();

        using var documentoFinal =
            JsonDocument.Parse(contenidoFinal);

        Assert.Equal(
            1,
            documentoFinal.RootElement
                .GetProperty("count")
                .GetInt32());

        var sesionActual =
            ObtenerSesionActualId(contenidoFinal);

        Assert.NotEqual(
            Guid.Empty,
            sesionActual);

        using var scope =
            factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<JurigestDbContext>();

        var acciones = await context.AuditoriasSeguridad
            .AsNoTracking()
            .Select(auditoria => auditoria.Accion)
            .ToListAsync();

        Assert.Contains(
            "SesionRevocada",
            acciones);

        Assert.Contains(
            "OtrasSesionesRevocadas",
            acciones);
    }

    private static Task<LoginResult> IniciarSesionAsync(
        HttpClient client)
    {
        return SeguridadTestHelper.IniciarSesionAsync(
            client,
            SeguridadTestHelper.AdminEmail,
            SeguridadTestHelper.AdminPassword);
    }

    private static Guid ObtenerSesionActualId(
        string contenido)
    {
        using var documento =
            JsonDocument.Parse(contenido);

        foreach (var sesion in documento.RootElement
            .GetProperty("value")
            .EnumerateArray())
        {
            if (sesion
                .GetProperty("esActual")
                .GetBoolean())
            {
                return sesion
                    .GetProperty("id")
                    .GetGuid();
            }
        }

        throw new InvalidOperationException(
            "No se encontro la sesion actual.");
    }
}