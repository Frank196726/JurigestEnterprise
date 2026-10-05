using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    public sealed record CrearRolRequest(string Nombre, RolUsuario Perfil);
}

