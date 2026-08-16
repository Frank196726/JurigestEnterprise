using System.Net;
using System.Net.Http.Json;
using Jurigest.Integration.Tests.Infrastructure;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class BloqueoCuentaTests
{
    [Fact]
    public async Task CincoPasswordsIncorrectas_BloqueanYAuditanCuenta()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        await SeguridadTestHelper
            .CrearAdministradorAsync(client);

        for (var intento = 1; intento <= 5; intento++)
        {
            using var response = await client.PostAsJsonAsync(
                "/api/seguridad/login",
                new
                {
                    email = SeguridadTestHelper.AdminEmail,
                    password = "Password.Incorrecta.2026!"
                });

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        using var scope =
            factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<JurigestDbContext>();

        var usuario = await context.Usuarios
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.NotNull(usuario.BloqueadoHastaUtc);
        Assert.True(
            usuario.BloqueadoHastaUtc > DateTime.UtcNow);

        var auditoria = await context.AuditoriasSeguridad
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(
            "UsuarioBloqueadoPorIntentosFallidos",
            auditoria.Accion);

        Assert.Null(auditoria.UsuarioActorId);

        Assert.Equal(
            usuario.Id,
            auditoria.UsuarioAfectadoId);
    }
}