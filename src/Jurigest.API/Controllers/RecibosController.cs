using Jurigest.API.Contracts;
using Jurigest.Application.Judicial.Recibos.Commands.MarcarPagado;
using Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibo;
using Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "DiligenciasLectura")]
public sealed class RecibosController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecibosController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerRecibosQuery(),
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obtener(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObtenerReciboQuery(id),
            cancellationToken);

        if (resultado is null)
        {
            return NotFound(new
            {
                mensaje = "El recibo no existe."
            });
        }

        return Ok(resultado);
    }

    [HttpPut("{id:guid}/pagar")]
    [Authorize(Policy = "DiligenciasGestion")]
    public async Task<IActionResult> MarcarPagado(
        Guid id,
        [FromBody] MarcarReciboPagadoRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            request.FechaPago == default)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar la fecha de pago."
            });
        }

        try
        {
            var resultado = await _mediator.Send(
                new MarcarReciboPagadoCommand(
                    id,
                    request.FechaPago),
                cancellationToken);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "El recibo no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Recibo marcado como pagado correctamente."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                mensaje = ex.Message
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
}
