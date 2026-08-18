using System.Net;
using System.Net.Http.Json;
using Jurigest.Web.Security;
using Microsoft.AspNetCore.Antiforgery;

namespace Jurigest.Web.Endpoints;

public static class SeguridadWebEndpoints
{
    public const string CookieName =
        "Jurigest.Session";

    public static IEndpointRouteBuilder MapSeguridadWebEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/auth/login",
            IniciarSesionAsync);

        endpoints.MapPost(
            "/auth/logout",
            CerrarSesionAsync);

        endpoints.MapPost(
            "/auth/password/recuperacion",
            SolicitarRecuperacionPasswordAsync);

        return endpoints;

    }

    private static async Task<IResult> IniciarSesionAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        ISesionWebStore sesionStore,
        IAntiforgery antiforgery,
        CancellationToken cancellationToken)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(
                context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Redirect(
                "/login?error=solicitud");
        }

        var formulario =
            await context.Request.ReadFormAsync(
                cancellationToken);

        var email =
            formulario["email"].ToString().Trim();

        var password =
            formulario["password"].ToString();

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return Results.Redirect(
                "/login?error=campos");
        }

        var client =
            httpClientFactory.CreateClient(
                "JurigestApi");

        using var response =
            await client.PostAsJsonAsync(
                "/api/seguridad/login",
                new
                {
                    email,
                    password
                },
                cancellationToken);

        if (response.StatusCode ==
            HttpStatusCode.TooManyRequests)
        {
            return Results.Redirect(
                "/login?error=limite");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Results.Redirect(
                "/login?error=credenciales");
        }

        var resultado =
            await response.Content
                .ReadFromJsonAsync<IniciarSesionApiResponse>(
                    cancellationToken);

        if (resultado is null)
        {
            return Results.Redirect(
                "/login?error=servicio");
        }

        var sesion = new SesionWeb(
            resultado.AccessToken,
            resultado.ExpiresAtUtc,
            resultado.RefreshToken,
            resultado.RefreshTokenExpiresAtUtc,
            resultado.UsuarioId,
            resultado.Nombre,
            resultado.Email,
            resultado.Rol);

        var identificador =
            sesionStore.Crear(sesion);

        context.Response.Cookies.Append(
            CookieName,
            identificador,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Path = "/",
                Expires = new DateTimeOffset(
                    resultado.RefreshTokenExpiresAtUtc)
            });

        return Results.Redirect("/");
    }
    private static async Task<IResult> CerrarSesionAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        ISesionWebStore sesionStore,
        IAntiforgery antiforgery,
        CancellationToken cancellationToken)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(
                context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest();
        }

        if (context.Request.Cookies.TryGetValue(
            CookieName,
            out var identificador))
        {
            var sesion =
                sesionStore.Obtener(identificador);

            if (sesion is not null)
            {
                try
                {
                    var client =
                        httpClientFactory.CreateClient(
                            "JurigestApi");

                    await client.PostAsJsonAsync(
                        "/api/seguridad/logout",
                        new
                        {
                            refreshToken = sesion.RefreshToken
                        },
                        cancellationToken);
                }
                catch (HttpRequestException)
                {
                    // El cierre local debe completarse aunque el API
                    // esté temporalmente fuera de servicio.
                }
            }

            sesionStore.Eliminar(identificador);
        }

        context.Response.Cookies.Delete(
        CookieName,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Path = "/"
        });

        return Results.Redirect("/login");
    }

    private static async Task<IResult>
        SolicitarRecuperacionPasswordAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        IAntiforgery antiforgery,
        CancellationToken cancellationToken)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(
                context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Redirect(
                "/password/recuperacion?error=solicitud");
        }

        var formulario =
            await context.Request.ReadFormAsync(
            cancellationToken);

        var email =
            formulario["email"].ToString().Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.Redirect(
                "/password/recuperacion?error=campos");
        }

        var client =
            httpClientFactory.CreateClient(
            "JurigestApi");

        try
        {
            using var response =
                await client.PostAsJsonAsync(
                    "/api/seguridad/password/recuperacion/solicitar",
                    new
                    {
                        email
                    },
                    cancellationToken);

            if (response.StatusCode ==
                HttpStatusCode.TooManyRequests)
            {
                return Results.Redirect(
                    "/password/recuperacion?error=limite");
            }

            if (!response.IsSuccessStatusCode)
            {
                return Results.Redirect(
                    "/password/recuperacion?error=servicio");
            }
        }
        catch (HttpRequestException)
        {
            return Results.Redirect(
            "/password/recuperacion?error=servicio");
        }

        return Results.Redirect(
    "/password/recuperacion?enviado=true");
    }

}