using Jurigest.API.Contracts;
using Jurigest.Application.Judicial.Resoluciones.Commands.RegistrarResolucion;
using Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucionesPorCausa;
using Jurigest.Application.Judicial.Resoluciones.Commands.EliminarResolucion;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucion;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api")]
public sealed class ResolucionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResolucionesController(IMediator mediator)
    {
        _mediator = mediator;
    }

        [Authorize(Policy = "ResolucionesRegistro")]
        [HttpPost("Causas/{causaId:guid}/resoluciones")]
        public async Task<IActionResult> Registrar(
            Guid causaId,
            [FromBody] RegistrarResolucionRequest request,
            CancellationToken cancellationToken)
    {
        if (request is null ||
            request.Fecha == default ||
            string.IsNullOrWhiteSpace(request.Descripcion))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar tipo, fecha y descripcion."
            });
        }

        if (!Enum.IsDefined(typeof(TipoResolucion), request.Tipo))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de resolucion no es valido."
            });
        }

        try
        {
            var resolucionId = await _mediator.Send(
                new RegistrarResolucionCommand(
                    causaId,
                    request.Tipo,
                    request.Fecha,
                    request.Descripcion),
                cancellationToken);

            if (resolucionId is null)
            {
                return NotFound(new
                {
                    mensaje = "La causa no existe."
                });
            }

            return Created(
                $"/api/Resoluciones/{resolucionId}",
                new
                {
                    id = resolucionId,
                    mensaje = "Resolucion registrada correctamente."
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

        [Authorize(Policy = "ResolucionesLectura")]
        [HttpGet("Resoluciones/{id:guid}")]
        public async Task<IActionResult> Obtener(
            Guid id,
            CancellationToken cancellationToken)
    {
        var resolucion = await _mediator.Send(
            new ObtenerResolucionQuery(id),
            cancellationToken);

        if (resolucion is null)
        {
            return NotFound(new
            {
                mensaje = "La resolucion no existe."
            });
        }

        return Ok(resolucion);
    }

        [Authorize(Policy = "ResolucionesLectura")]
        [HttpGet("Causas/{causaId:guid}/resoluciones")]
        public async Task<IActionResult> ObtenerPorCausa(
            Guid causaId,
            CancellationToken cancellationToken)
    {
        var resoluciones = await _mediator.Send(
            new ObtenerResolucionesPorCausaQuery(causaId),
            cancellationToken);

        return Ok(resoluciones);
    }

        [Authorize(Policy = "ResolucionesEliminacion")]
        [HttpDelete("Resoluciones/{id:guid}")]
        public async Task<IActionResult> Eliminar(
            Guid id,
            CancellationToken cancellationToken)
    {
        var eliminada = await _mediator.Send(
            new EliminarResolucionCommand(id),
            cancellationToken);

        if (!eliminada)
        {
            return NotFound(new
            {
                mensaje = "La resolucion no existe."
            });
        }

        return Ok(new
        {
            mensaje = "Resolucion eliminada correctamente."
        });
    }
}