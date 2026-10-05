using System.Security.Claims;
using System.Text.Json;
using Jurigest.Domain.Seguridad;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using Jurigest.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.API.Security;

public sealed record PermisoRequirement(string Codigo) : IAuthorizationRequirement;

public sealed class PermisosService(JurigestDbContext db)
{
    public async Task<string[]> ObtenerAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return [];
        var usuario = await db.Usuarios.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (usuario is null || !usuario.Activo || usuario.DebeCambiarPassword) return [];
        if (usuario.Rol == RolUsuario.Administrador && usuario.RolCatalogoId is null)
            return PermisosSistema.Catalogo.Select(x => x.Codigo).ToArray();
        var clave = usuario.RolCatalogoId?.ToString() ?? usuario.Rol.ToString();
        var configuracion = await db.Set<PermisosRol>().AsNoTracking().SingleOrDefaultAsync(x => x.Clave == clave, ct);
        var perfil = usuario.Rol == RolUsuario.Administrador ? RolUsuario.Consulta : usuario.Rol;
        return configuracion is null ? PermisosSistema.Predeterminados(perfil) :
            (JsonSerializer.Deserialize<string[]>(configuracion.SeleccionJson) ?? [])
            .Where(x => x != "Administracion").ToArray();
    }
}

public sealed class PermisosAuthorizationHandler(PermisosService permisos, IHttpContextAccessor accessor)
    : AuthorizationHandler<PermisoRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisoRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            (await permisos.ObtenerAsync(context.User, accessor.HttpContext?.RequestAborted ?? default)).Contains(requirement.Codigo))
            context.Succeed(requirement);
    }
}
