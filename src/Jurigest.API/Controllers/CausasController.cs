using Jurigest.API.Contracts;
using Jurigest.Application.Judicial.Causas.Commands.ActualizarCausa;
using Jurigest.Application.Judicial.Causas.Commands.CrearCausa;
using Jurigest.Application.Judicial.Causas.Commands.AgregarDemandado;
using Jurigest.Application.Judicial.Causas.Commands.AgregarAvalSolidario;
using Jurigest.Application.Judicial.Causas.Commands.ActualizarIdentificacionParte;
using Jurigest.Application.Judicial.Causas.Commands.EliminarCausa;
using Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;
using Jurigest.Application.Judicial.Causas.Queries.ObtenerCausas;
using Jurigest.Application.Judicial.Causas.Queries.ObtenerCausa;
using Jurigest.Application.Judicial.Panel.Queries.ObtenerPanelEjecutivo;
using Jurigest.Application.Judicial.Reportes.Queries.ObtenerReporteCausas;
using Jurigest.Application.Judicial.Diligencias.Commands.CrearDiligencia;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CausasController : ControllerBase
{
    private readonly IMediator _mediator;

    public CausasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearCausaRequest request,
        [FromServices] Jurigest.Persistence.Context.JurigestDbContext db,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                mensaje = "La solicitud no contiene datos."
            });
        }

        var materia = request.MateriaId.HasValue ? await db.Materias.FindAsync(new object[] { request.MateriaId.Value }, cancellationToken) : null;
        if (request.MateriaId.HasValue && materia is null) return BadRequest(new { mensaje = "La materia seleccionada no existe." });
        var command = new CrearCausaCommand
        {
            Id = request.Id,
            MateriaId = materia?.Id,
            Materia = materia?.Nombre,
            Rit = request.Rit,
            TipoCausaId = request.TipoCausaId,
            NumeroRol = request.NumeroRol,
            Tribunal = request.Tribunal,
            Descripcion = request.Descripcion,
            FechaEncargoCausa = request.FechaEncargoCausa,
            Demandados = (request.Demandados ?? []).Select(x => new DemandadoInput(
                x.Nombre, x.TipoPersona, x.EsPrincipal,
                (x.Avales ?? []).Select(a => new AvalSolidarioInput(a.Nombre, a.TipoPersona,
                    a.Rut, a.RepresentanteLegal, a.RutRepresentanteLegal)).ToList(),
                x.Rut, x.RepresentanteLegal, x.RutRepresentanteLegal)).ToList()
        };

        try
        {
            var resultado = await _mediator.Send(command, cancellationToken);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerCausasQuery(ObtenerFiltroReceptor()),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerCausaQuery(id, ObtenerFiltroReceptor()),
            cancellationToken);

        if (resultado is null)
        {
            return NotFound(new
            {
                mensaje = "La causa no existe."
            });
        }

        return Ok(resultado);
    }

    [HttpPost("{causaId:guid}/demandados")]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> AgregarDemandado(
        Guid causaId, [FromBody] AgregarParteRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { mensaje = "Debe indicar el demandado." });
        try
        {
            var id = await _mediator.Send(new AgregarDemandadoCommand(
                causaId, request.Nombre, request.TipoPersona, request.EsPrincipal,
                request.Rut, request.RepresentanteLegal, request.RutRepresentanteLegal), cancellationToken);
            return id is null ? NotFound() : Ok(new { id });
        }
        catch (InvalidOperationException ex) { return Conflict(new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpPost("{causaId:guid}/demandados/{demandadoId:guid}/avales")]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> AgregarAvalSolidario(
        Guid causaId, Guid demandadoId, [FromBody] AgregarParteRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { mensaje = "Debe indicar el aval solidario." });
        try
        {
            var id = await _mediator.Send(new AgregarAvalSolidarioCommand(
                causaId, demandadoId, request.Nombre, request.TipoPersona,
                request.Rut, request.RepresentanteLegal, request.RutRepresentanteLegal), cancellationToken);
            return id is null ? NotFound() : Ok(new { id });
        }
        catch (InvalidOperationException ex) { return Conflict(new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpPut("{causaId:guid}/demandados/{demandadoId:guid}/identificacion")]
    [Authorize(Policy = "CausasEscritura")]
    public Task<IActionResult> ActualizarDemandado(
        Guid causaId, Guid demandadoId, [FromBody] ActualizarIdentificacionParteRequest request,
        CancellationToken cancellationToken) =>
        ActualizarIdentificacion(causaId, demandadoId, null, request, cancellationToken);

    [HttpPut("{causaId:guid}/demandados/{demandadoId:guid}/avales/{avalId:guid}/identificacion")]
    [Authorize(Policy = "CausasEscritura")]
    public Task<IActionResult> ActualizarAval(
        Guid causaId, Guid demandadoId, Guid avalId,
        [FromBody] ActualizarIdentificacionParteRequest request, CancellationToken cancellationToken) =>
        ActualizarIdentificacion(causaId, demandadoId, avalId, request, cancellationToken);

    private async Task<IActionResult> ActualizarIdentificacion(
        Guid causaId, Guid demandadoId, Guid? avalId,
        ActualizarIdentificacionParteRequest request, CancellationToken cancellationToken)
    {
        if (request is null) return BadRequest(new { mensaje = "Debe indicar los datos de identificación." });
        try
        {
            var actualizado = await _mediator.Send(new ActualizarIdentificacionParteCommand(
                causaId, demandadoId, avalId, request.Rut,
                request.RepresentanteLegal, request.RutRepresentanteLegal), cancellationToken);
            return actualizado ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpGet("rit/{rit}")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> BuscarPorRit(
        string rit,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new BuscarCausaPorRitQuery(rit, ObtenerFiltroReceptor()),
            cancellationToken);

        if (resultado is null)
        {
            return NotFound(new
            {
                mensaje = "No existe una causa con ese RIT."
            });
        }

        return Ok(resultado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CausasEscritura")]
    public async Task<IActionResult> Actualizar(
    Guid id,
    [FromBody] ActualizarCausaRequest request,
    CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                mensaje = "La solicitud no contiene datos."
            });
        }

        var command = new ActualizarCausaCommand
        {
            Id = id,
            Tribunal = request.Tribunal,
            Descripcion = request.Descripcion,
            FechaEncargoCausa = request.FechaEncargoCausa,
            DiligenciaId = request.DiligenciaId,
            DiligenciaEncargadaId = request.DiligenciaEncargadaId,
            FechaProgramada = request.FechaProgramada,
            ReceptorJudicial = request.ReceptorJudicial
        };

        try
        {
            var resultado = await _mediator.Send(command, cancellationToken);
            return resultado is null ? NotFound() : Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CausasEliminacion")]
    public async Task<IActionResult> Eliminar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new EliminarCausaCommand(id),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("{causaId:guid}/diligencias")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> CrearDiligencia(
        Guid causaId,
        [FromBody] CrearDiligenciaRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Descripcion))
        {
            return BadRequest(new
            {
                mensaje = "La descripción de la diligencia es obligatoria."
            });
        }

        var command = new CrearDiligenciaCommand(
            causaId,
            request.Descripcion,
            request.Tipo,
            ObtenerFiltroReceptor());

        var id = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(new
        {
            id,
            mensaje = "Diligencia creada correctamente."
        });
    }

    [HttpGet("reporte")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerReporte(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new ObtenerReporteCausasQuery(ObtenerFiltroReceptor()), cancellationToken));
    }

    [HttpGet("panel-ejecutivo")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerPanelEjecutivo(
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerPanelEjecutivoQuery(ObtenerFiltroReceptor()),
            cancellationToken);

        return Ok(resultado);
    }

    private string? ObtenerFiltroReceptor()
    {
        var rolAsignado = User.FindFirst("rol_asignado")?.Value;
        return string.Equals(rolAsignado, "Receptor", StringComparison.OrdinalIgnoreCase)
            ? User.Identity?.Name
            : null;
    }
}
