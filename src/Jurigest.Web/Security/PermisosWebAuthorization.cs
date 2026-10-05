using Microsoft.AspNetCore.Authorization;

namespace Jurigest.Web.Security;

public sealed record PermisoWebRequirement(string Codigo) : IAuthorizationRequirement;
public sealed class PermisosWebAuthorizationHandler(IJurigestApiClient api, ILogger<PermisosWebAuthorizationHandler> logger) : AuthorizationHandler<PermisoWebRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisoWebRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true) return;
        try
        {
            var permisos = await api.GetAsync<string[]>("/api/seguridad/permisos/mios");
            if (permisos?.Contains(requirement.Codigo) == true) context.Succeed(requirement);
        }
        catch (UnauthorizedAccessException)
        {
            context.Fail(new AuthorizationFailureReason(this, "sesion_expirada"));
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            logger.LogWarning(ex, "No se pudieron comprobar los permisos de la sesión web.");
            context.Fail(new AuthorizationFailureReason(this, "permisos_no_disponibles"));
        }
    }
}
