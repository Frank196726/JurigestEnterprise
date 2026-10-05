using System.Security.Claims;
using System.Text.Json;
using Jurigest.API.Security;
using Jurigest.Domain.Seguridad;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using Jurigest.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/seguridad/permisos")]
[Authorize]
public sealed class PermisosController(JurigestDbContext db, PermisosService permisos) : ControllerBase
{
    [HttpGet("mios")]
    public async Task<IActionResult> Mios(CancellationToken ct) => Ok(await permisos.ObtenerAsync(User, ct));

    [HttpGet]
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var roles = Enum.GetValues<RolUsuario>().Select(x => new RolItem(x.ToString(), x.ToString(), x)).ToList();
        roles.AddRange((await db.Set<RolCatalogo>().AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(ct))
            .Select(x => new RolItem(x.Id.ToString(), x.Nombre, x.Perfil == RolUsuario.Administrador ? RolUsuario.Consulta : x.Perfil)));
        var guardados = await db.Set<PermisosRol>().AsNoTracking().ToDictionaryAsync(x => x.Clave, ct);
        return Ok(new
        {
            catalogo = PermisosSistema.Catalogo,
            roles = roles.Select(r => new
            {
                r.Clave,
                r.Nombre,
                bloqueado = r.Clave == "Administrador",
                version = guardados.GetValueOrDefault(r.Clave)?.Version ?? Guid.Empty,
                seleccion = r.Clave == "Administrador" ? PermisosSistema.Predeterminados(RolUsuario.Administrador) :
                guardados.TryGetValue(r.Clave, out var c) ? JsonSerializer.Deserialize<string[]>(c.SeleccionJson) : PermisosSistema.Predeterminados(r.Perfil)
            })
        });
    }

    [HttpPut("{clave}")]
    [Authorize(Policy = "Administracion")]
    public async Task<IActionResult> Guardar(string clave, GuardarRequest request, CancellationToken ct)
    {
        if (clave == "Administrador") return BadRequest(new { mensaje = "Administrador conserva todos los permisos y no se puede modificar." });
        var esBase = Enum.TryParse<RolUsuario>(clave, out var perfil) && Enum.IsDefined(perfil) && perfil.ToString() == clave;
        if (!esBase && !(Guid.TryParse(clave, out var id) && await db.Set<RolCatalogo>().AnyAsync(x => x.Id == id, ct))) return NotFound();
        if (request.Seleccion is null) return BadRequest(new { mensaje = "Debe enviar una selección de permisos." });
        try { PermisosSistema.Validar(request.Seleccion); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        var actual = await db.Set<PermisosRol>().SingleOrDefaultAsync(x => x.Clave == clave, ct);
        if ((actual?.Version ?? Guid.Empty) != request.Version) return Conflict(new { mensaje = "Otro administrador cambió estos permisos. Recarga antes de guardar." });
        if (actual is null) { actual = new PermisosRol { Clave = clave }; db.Add(actual); }
        actual.SeleccionJson = JsonSerializer.Serialize(request.Seleccion.Distinct().Order().ToArray());
        actual.Version = Guid.NewGuid();
        db.AuditoriasSeguridad.Add(new AuditoriaSeguridad(Guid.NewGuid(), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            "PermisosRolActualizados", null, $"Rol: {clave}. Permisos: {string.Join(", ", request.Seleccion)}", HttpContext.Connection.RemoteIpAddress?.ToString()));
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "No se pudieron guardar los permisos. Recarga e inténtalo nuevamente." }); }
        return Ok(new { actual.Version, mensaje = "Permisos guardados. Se aplican a todos los usuarios del rol en su próxima operación." });
    }
    public sealed record GuardarRequest(string[]? Seleccion, Guid Version);
    private sealed record RolItem(string Clave, string Nombre, RolUsuario Perfil);
}
