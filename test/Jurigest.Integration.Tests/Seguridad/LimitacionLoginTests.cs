using System.Net;
using System.Net.Http.Json;
using Jurigest.Integration.Tests.Infrastructure;

namespace Jurigest.Integration.Tests.Seguridad;

public sealed class LimitacionLoginTests
{
    [Fact]
    public async Task Login_SextoIntentoEnUnMinuto_Devuelve429()
    {
        await using var factory =
            new JurigestApiFactory();

        using var client = factory.CreateClient();

        for (var intento = 1; intento <= 5; intento++)
        {
            using var response = await client.PostAsJsonAsync(
                "/api/seguridad/login",
                new
                {
                    email = "inexistente@jurigest.test",
                    password = "Password.Incorrecta.2026!"
                });

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        using var bloqueado = await client.PostAsJsonAsync(
            "/api/seguridad/login",
            new
            {
                email = "inexistente@jurigest.test",
                password = "Password.Incorrecta.2026!"
            });

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            bloqueado.StatusCode);
    }
}