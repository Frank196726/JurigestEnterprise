using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Jurigest.Web.Security;

public sealed class PermisosAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult result)
    {
        var razones = result.AuthorizationFailure?.FailureReasons.Select(x => x.Message).ToArray() ?? [];
        if (razones.Contains("sesion_expirada"))
        {
            context.Response.Redirect("/login?error=sesion");
            return;
        }
        if (razones.Contains("permisos_no_disponibles"))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync("<!doctype html><html lang=\"es\"><meta charset=\"utf-8\"><title>Servicio no disponible</title><main><h1>No se pudieron comprobar tus permisos</h1><p>Verifica que la API esté iniciada y vuelve a cargar la página.</p><a href=\"/\">Volver al inicio</a></main></html>");
            return;
        }
        await _default.HandleAsync(next, context, policy, result);
    }
}
