using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/modelos-estampe")]
[Authorize]
public sealed class ModelosEstampeController(IModeloEstampeRepository repository,
    IDiligenciaRealizadaCatalogoRepository diligenciasRealizadas) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "DiligenciasLectura")]
    public async Task<IActionResult> Obtener([FromQuery] int? tipoDiligencia,
        [FromQuery] int? resultado, [FromQuery] bool incluirInactivos,
        CancellationToken cancellationToken)
    {
        var modelos = incluirInactivos
            ? await repository.GetTodosAsync(cancellationToken)
            : await repository.GetActivosAsync(cancellationToken);
        if (tipoDiligencia.HasValue)
            modelos = modelos.Where(x => (int)x.TipoDiligencia == tipoDiligencia.Value).ToList();
        if (resultado.HasValue)
            modelos = modelos.Where(x => x.Resultado is null || (int)x.Resultado == resultado.Value).ToList();
        return Ok(modelos.Select(Mapear));
    }

    [HttpPost]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> Crear([FromBody] GuardarModeloEstampeRequest request,
        CancellationToken cancellationToken)
    {
        var errorRelacion = await ValidarRelacionAsync(request, cancellationToken);
        if (errorRelacion is not null) return errorRelacion;
        if (await repository.ExisteNombreAsync(request.Nombre, null, cancellationToken))
            return Conflict(new { mensaje = "Ya existe un modelo de estampe con ese nombre." });
        try
        {
            var modelo = new ModeloEstampe(Guid.NewGuid(), request.Nombre,
                (TipoDiligencia)request.TipoDiligencia, request.Contenido,
                request.Resultado.HasValue ? (ResultadoDiligencia?)request.Resultado.Value : null,
                request.DiligenciaRealizadaId);
            await repository.AddAsync(modelo, cancellationToken);
            return Ok(Mapear(modelo));
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] GuardarModeloEstampeRequest request,
        CancellationToken cancellationToken)
    {
        var modelo = await repository.GetByIdAsync(id, cancellationToken);
        if (modelo is null) return NotFound();
        var errorRelacion = await ValidarRelacionAsync(request, cancellationToken);
        if (errorRelacion is not null) return errorRelacion;
        if (await repository.ExisteNombreAsync(request.Nombre, id, cancellationToken))
            return Conflict(new { mensaje = "Ya existe un modelo de estampe con ese nombre." });
        try
        {
            modelo.Actualizar(request.Nombre, (TipoDiligencia)request.TipoDiligencia,
                request.Contenido, request.Resultado.HasValue
                    ? (ResultadoDiligencia?)request.Resultado.Value : null,
                request.DiligenciaRealizadaId);
            if (request.Activo) modelo.Activar(); else modelo.Desactivar();
            await repository.SaveChangesAsync(cancellationToken);
            return Ok(Mapear(modelo));
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken cancellationToken)
    {
        var modelo = await repository.GetByIdAsync(id, cancellationToken);
        if (modelo is null) return NotFound();
        modelo.Desactivar();
        await repository.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static object Mapear(ModeloEstampe x) => new
    {
        x.Id,
        x.Nombre,
        TipoDiligencia = (int)x.TipoDiligencia,
        Resultado = x.Resultado.HasValue ? (int?)x.Resultado.Value : null,
        x.DiligenciaRealizadaId,
        x.Contenido,
        x.Activo,
        x.FechaCreacion,
        x.FechaActualizacion
    };

    private async Task<IActionResult?> ValidarRelacionAsync(
        GuardarModeloEstampeRequest request, CancellationToken cancellationToken)
    {
        if (!request.DiligenciaRealizadaId.HasValue) return null;
        var gestion = await diligenciasRealizadas.GetByIdAsync(
            request.DiligenciaRealizadaId.Value, cancellationToken);
        if (gestion is null || !gestion.Activo)
            return BadRequest(new { mensaje = "La gestión realizada seleccionada no existe o no está activa." });
        if (gestion.CodigoTipoDiligencia != request.TipoDiligencia)
            return BadRequest(new { mensaje = "La gestión realizada no corresponde al tipo de diligencia seleccionado." });
        return null;
    }

    public sealed class GuardarModeloEstampeRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public int TipoDiligencia { get; set; }
        public int? Resultado { get; set; }
        public Guid? DiligenciaRealizadaId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
