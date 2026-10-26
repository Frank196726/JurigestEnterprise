using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using Microsoft.AspNetCore.Authorization;
using Jurigest.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/seguridad/roles")]
[Authorize(Policy = "Administracion")]
public sealed class RolesController(IRolCatalogoRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct) => Ok(
        (await repository.GetAllAsync(ct)).Select(x => new { x.Id, x.Nombre, perfil = (int)x.Perfil }));

    [HttpPost]
    public async Task<IActionResult> Crear(CrearRolRequest request, CancellationToken ct)
    {
        try
        {
            var rol = new RolCatalogo(Guid.NewGuid(), request.Nombre, request.Perfil);
            if (!await repository.TryAddAsync(rol, ct))
                return Conflict(new { mensaje = "Ya existe un rol con ese nombre." });
            return Ok(new { rol.Id, rol.Nombre, perfil = (int)rol.Perfil });
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.MensajeParaUsuario() }); }
    }

    /// <summary>
    /// Cambia el perfil base de un rol y lo aplica a sus usuarios. Si el rol tenía permisos
    /// propios se descartan, para que tome los predeterminados del nuevo perfil.
    /// </summary>
    [HttpPut("{id:guid}/perfil")]
    public async Task<IActionResult> CambiarPerfil(Guid id, CambiarPerfilRequest request,
        [FromServices] JurigestDbContext db, CancellationToken ct)
    {
        var rol = await db.Set<RolCatalogo>().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (rol is null) return NotFound(new { mensaje = "El rol no existe." });
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId)) return Unauthorized();

        var anterior = rol.Perfil;
        try { rol.CambiarPerfil(request.Perfil); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.MensajeParaUsuario() }); }
        if (anterior == rol.Perfil)
            return Ok(new { rol.Id, rol.Nombre, perfil = (int)rol.Perfil, usuariosAfectados = 0, permisosRestablecidos = false });

        // AsignarRolCatalogo actualiza el perfil del usuario e invalida sus sesiones.
        var usuarios = await db.Usuarios.Where(x => x.RolCatalogoId == id).ToListAsync(ct);
        foreach (var usuario in usuarios) usuario.AsignarRolCatalogo(rol);

        var permisos = await db.Set<PermisosRol>().SingleOrDefaultAsync(x => x.Clave == id.ToString(), ct);
        if (permisos is not null) db.Remove(permisos);

        db.AuditoriasSeguridad.Add(new AuditoriaSeguridad(Guid.NewGuid(), actorId, "RolPerfilCambiado", null,
            $"Rol: {rol.Nombre}. Perfil: {anterior} → {rol.Perfil}. Usuarios: {usuarios.Count}." +
            (permisos is null ? string.Empty : " Se restablecieron sus permisos a los del nuevo perfil."),
            HttpContext.Connection.RemoteIpAddress?.ToString()));
        await db.SaveChangesAsync(ct);

        return Ok(new { rol.Id, rol.Nombre, perfil = (int)rol.Perfil, usuariosAfectados = usuarios.Count, permisosRestablecidos = permisos is not null });
    }

    public sealed record CrearRolRequest(string Nombre, RolUsuario Perfil);
    public sealed record CambiarPerfilRequest(RolUsuario Perfil);
}

