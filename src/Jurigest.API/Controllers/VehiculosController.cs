using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Jurigest.Application.Judicial.Causas.Queries.ObtenerCausa;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/diligencias/{diligenciaId:guid}/vehiculos")]
[Authorize(Policy = "DiligenciasLectura")]
public sealed class VehiculosController(JurigestDbContext db, IMediator mediator) : ControllerBase
{
    private async Task<bool> Acceso(Guid id, CancellationToken ct)
    {
        var diligencia = await db.Diligencias.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (diligencia is null) return false;
        var filtro = string.Equals(User.FindFirst("rol_asignado")?.Value, "Receptor", StringComparison.OrdinalIgnoreCase) ? User.Identity?.Name : null;
        return await mediator.Send(new ObtenerCausaQuery(diligencia.CausaId, filtro), ct) is not null;
    }
    [HttpGet]
    public async Task<IActionResult> Listar(Guid diligenciaId, CancellationToken ct)
    {
        if (!await Acceso(diligenciaId, ct)) return NotFound();
        return Ok(await db.VehiculosEncargo.AsNoTracking().Where(x => x.DiligenciaId == diligenciaId).OrderBy(x => x.Patente).ToListAsync(ct));
    }
    [HttpPost]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> Crear(Guid diligenciaId, VehiculoEncargo datos, CancellationToken ct) => await Guardar(diligenciaId, null, datos, ct);
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> Editar(Guid diligenciaId, Guid id, VehiculoEncargo datos, CancellationToken ct) => await Guardar(diligenciaId, id, datos, ct);
    private async Task<IActionResult> Guardar(Guid diligenciaId, Guid? id, VehiculoEncargo datos, CancellationToken ct)
    {
        if (!await Acceso(diligenciaId, ct)) return NotFound();
        datos.Color ??= ""; datos.Cilindrada ??= ""; datos.Motor ??= ""; datos.Chasis ??= ""; datos.Observaciones ??= "";
        if (string.IsNullOrWhiteSpace(datos.Patente) || datos.Patente.Length > 20 ||
            string.IsNullOrWhiteSpace(datos.Nombre) || datos.Nombre.Length > 300 ||
            string.IsNullOrWhiteSpace(datos.Direccion) || datos.Direccion.Length > 500 ||
            string.IsNullOrWhiteSpace(datos.Marca) || datos.Marca.Length > 100 ||
            string.IsNullOrWhiteSpace(datos.Modelo) || datos.Modelo.Length > 200 ||
            datos.Color.Length > 100 || datos.Cilindrada.Length > 50 || datos.Motor.Length > 100 ||
            datos.Chasis.Length > 100 || datos.Observaciones.Length > 1000 || datos.Ano is < 1900 or > 2100)
            return BadRequest(new { mensaje = "Revise los datos obligatorios y sus longitudes." });

        if (datos.TipoPropietario is < 1 or > 4 || datos.DerechosInscripcion < 0 || datos.DerechosInscripcion > 999999999999999999m || decimal.Truncate(datos.DerechosInscripcion) != datos.DerechosInscripcion)
            return BadRequest(new { mensaje = "Revise tipo de propietario y derechos de inscripción." });
        datos.TipoVehiculo ??= ""; if (datos.TipoVehiculo.Length > 200) return BadRequest(new { mensaje = "El campo TipoVehiculo supera 200 caracteres." });
        datos.Serie ??= ""; if (datos.Serie.Length > 200) return BadRequest(new { mensaje = "El campo Serie supera 200 caracteres." });
        datos.TipoAdquisicion ??= ""; if (datos.TipoAdquisicion.Length > 200) return BadRequest(new { mensaje = "El campo TipoAdquisicion supera 200 caracteres." });
        datos.AlzamientoProhibicion ??= ""; if (datos.AlzamientoProhibicion.Length > 200) return BadRequest(new { mensaje = "El campo AlzamientoProhibicion supera 200 caracteres." });
        datos.LimitacionDominio ??= ""; if (datos.LimitacionDominio.Length > 200) return BadRequest(new { mensaje = "El campo LimitacionDominio supera 200 caracteres." });
        datos.LugarSolicitud ??= ""; if (datos.LugarSolicitud.Length > 200) return BadRequest(new { mensaje = "El campo LugarSolicitud supera 200 caracteres." });
        datos.NumeroSolicitud ??= ""; if (datos.NumeroSolicitud.Length > 200) return BadRequest(new { mensaje = "El campo NumeroSolicitud supera 200 caracteres." });
        datos.TipoDocumento ??= ""; if (datos.TipoDocumento.Length > 200) return BadRequest(new { mensaje = "El campo TipoDocumento supera 200 caracteres." });
        datos.RutTitular ??= ""; if (datos.RutTitular.Length > 200) return BadRequest(new { mensaje = "El campo RutTitular supera 200 caracteres." });
        var patente = datos.Patente.Trim().ToUpperInvariant();
        if (await db.VehiculosEncargo.AnyAsync(x => x.DiligenciaId == diligenciaId && x.Patente == patente && (!id.HasValue || x.Id != id.Value), ct))
            return Conflict(new { mensaje = "Esta patente ya está registrada en el encargo." });
        var vehiculo = id.HasValue ? await db.VehiculosEncargo.FirstOrDefaultAsync(x => x.Id == id && x.DiligenciaId == diligenciaId, ct) : new VehiculoEncargo { Id = Guid.NewGuid(), DiligenciaId = diligenciaId };
        if (vehiculo is null) return NotFound();
        vehiculo.Patente = patente; vehiculo.Nombre = datos.Nombre.Trim(); vehiculo.Direccion = datos.Direccion.Trim();
        vehiculo.Marca = datos.Marca.Trim(); vehiculo.Modelo = datos.Modelo.Trim(); vehiculo.Color = datos.Color.Trim();
        vehiculo.Cilindrada = datos.Cilindrada.Trim(); vehiculo.Ano = datos.Ano; vehiculo.Motor = datos.Motor.Trim();
        vehiculo.Chasis = datos.Chasis.Trim(); vehiculo.Observaciones = datos.Observaciones.Trim();
        vehiculo.TipoPropietario = datos.TipoPropietario;
        vehiculo.TipoVehiculo = datos.TipoVehiculo.Trim();
        vehiculo.Serie = datos.Serie.Trim();
        vehiculo.FechaDocumento = datos.FechaDocumento;
        vehiculo.TipoAdquisicion = datos.TipoAdquisicion.Trim();
        vehiculo.AlzamientoProhibicion = datos.AlzamientoProhibicion.Trim();
        vehiculo.LimitacionDominio = datos.LimitacionDominio.Trim();
        vehiculo.LugarSolicitud = datos.LugarSolicitud.Trim();
        vehiculo.NumeroSolicitud = datos.NumeroSolicitud.Trim();
        vehiculo.TipoDocumento = datos.TipoDocumento.Trim();
        vehiculo.DerechosInscripcion = datos.DerechosInscripcion;
        vehiculo.RutTitular = datos.RutTitular.Trim();
        if (!id.HasValue) db.VehiculosEncargo.Add(vehiculo);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number is 2601 or 2627)
        { return Conflict(new { mensaje = "Esta patente ya está registrada en el encargo." }); }
        return Ok(vehiculo);
    }
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> Eliminar(Guid diligenciaId, Guid id, CancellationToken ct)
    {
        if (!await Acceso(diligenciaId, ct)) return NotFound();
        var vehiculo = await db.VehiculosEncargo.FirstOrDefaultAsync(x => x.Id == id && x.DiligenciaId == diligenciaId, ct);
        if (vehiculo is null) return NotFound();
        db.VehiculosEncargo.Remove(vehiculo); await db.SaveChangesAsync(ct); return NoContent();
    }
}
