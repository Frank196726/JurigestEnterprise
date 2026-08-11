using Jurigest.API.Contracts;
using Jurigest.Application.Judicial.Documentos.Commands.RegistrarDocumento;
using Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumento;
using Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumentosPorCausa;
using Jurigest.Application.Judicial.Documentos.Commands.EliminarDocumento;
using Jurigest.Domain.Judicial.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api")]
public sealed class DocumentosController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("Causas/{causaId:guid}/documentos")]
    public async Task<IActionResult> Registrar(
        Guid causaId,
        [FromBody] RegistrarDocumentoRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.RutaArchivo))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar nombre, tipo y ruta del documento."
            });
        }

        if (!Enum.IsDefined(typeof(TipoDocumento), request.Tipo))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de documento no es valido."
            });
        }

        try
        {
            var documentoId = await _mediator.Send(
                new RegistrarDocumentoCommand(
                    causaId,
                    request.Nombre,
                    request.Tipo,
                    request.RutaArchivo),
                cancellationToken);

            if (documentoId is null)
            {
                return NotFound(new
                {
                    mensaje = "La causa no existe."
                });
            }

            return Created(
                $"/api/Documentos/{documentoId}",
                new
                {
                    id = documentoId,
                    mensaje = "Documento registrado correctamente."
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
        [HttpGet("Documentos/{id:guid}")]
        public async Task<IActionResult> Obtener(
            Guid id,
            CancellationToken cancellationToken)
    {
        var documento = await _mediator.Send(
            new ObtenerDocumentoQuery(id),
            cancellationToken);

        if (documento is null)
        {
            return NotFound(new
            {
                mensaje = "El documento no existe."
            });
        }

        return Ok(documento);
    }

        [HttpGet("Causas/{causaId:guid}/documentos")]
        public async Task<IActionResult> ObtenerPorCausa(
            Guid causaId,
            CancellationToken cancellationToken)
    {
        var documentos = await _mediator.Send(
            new ObtenerDocumentosPorCausaQuery(causaId),
            cancellationToken);

        return Ok(documentos);
    }

        [HttpDelete("Documentos/{id:guid}")]
        public async Task<IActionResult> Eliminar(
            Guid id,
            CancellationToken cancellationToken)
    {
        var eliminado = await _mediator.Send(
            new EliminarDocumentoCommand(id),
            cancellationToken);

        if (!eliminado)
        {
            return NotFound(new
            {
                mensaje = "El documento no existe."
            });
        }

        return Ok(new
        {
            mensaje = "Documento eliminado correctamente."
        });
    }
}