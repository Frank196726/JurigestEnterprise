using Jurigest.API.Contracts;
using Jurigest.Application.Judicial.Causas.Commands.ActualizarCausa;
using Jurigest.Application.Judicial.Causas.Commands.CrearCausa;
using Jurigest.Application.Judicial.Causas.Commands.EliminarCausa;
using Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;
using Jurigest.Application.Judicial.Causas.Queries.ObtenerCausas;
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
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                mensaje = "La solicitud no contiene datos."
            });
        }

        var command = new CrearCausaCommand
        {
            Id = request.Id,
            Rit = request.Rit,
            Tribunal = request.Tribunal,
            Descripcion = request.Descripcion
        };

        var resultado = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerCausasQuery(),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("rit/{rit}")]
    [Authorize(Policy = "CausasLectura")]
    public async Task<IActionResult> BuscarPorRit(
        string rit,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new BuscarCausaPorRitQuery(rit),
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
            Descripcion = request.Descripcion
        };

        var resultado = await _mediator.Send(
            command,
            cancellationToken);

        if (resultado is null)
        {
            return NotFound();
        }

        return Ok(resultado);
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
            request.Descripcion);

        var id = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(new
        {
            id,
            mensaje = "Diligencia creada correctamente."
        });
    }
    }