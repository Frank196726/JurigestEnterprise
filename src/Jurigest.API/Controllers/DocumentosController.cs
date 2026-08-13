using Jurigest.API.Contracts;
using Jurigest.Application.Abstractions.Storage;
using Jurigest.Application.Judicial.Documentos.Commands.CargarDocumento;
using Jurigest.Application.Judicial.Documentos.Commands.EliminarDocumento;
using Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumento;
using Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumentosPorCausa;
using Microsoft.AspNetCore.Authorization;
using Jurigest.Domain.Judicial.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api")]
public sealed class DocumentosController : ControllerBase
{
        private readonly IMediator _mediator;
        private readonly IArchivoStorage _archivoStorage;

        public DocumentosController(
            IMediator mediator,
            IArchivoStorage archivoStorage)
{
            _mediator = mediator;
            _archivoStorage = archivoStorage;
}

    [HttpPost("Causas/{causaId:guid}/documentos/archivo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11_534_336)]
    [Authorize(Policy = "DocumentosCarga")]
    public async Task<IActionResult> CargarArchivo(
        Guid causaId,
        [FromForm] CargarDocumentoRequest request,
        CancellationToken cancellationToken)
    {
        const long maximoBytes = 10 * 1024 * 1024;

        if (request is null ||
            string.IsNullOrWhiteSpace(request.Nombre) ||
            request.Archivo is null ||
            request.Archivo.Length == 0)
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar nombre, tipo y archivo."
            });
        }

        if (!Enum.IsDefined(typeof(TipoDocumento), request.Tipo))
        {
            return BadRequest(new
            {
                mensaje = "El tipo de documento no es valido."
            });
        }

        if (request.Archivo.Length > maximoBytes)
        {
            return BadRequest(new
            {
                mensaje = "El archivo supera el limite de 10 MB."
            });
        }

        var extension = Path
            .GetExtension(request.Archivo.FileName)
            .ToLowerInvariant();

        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" =>
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => null
        };

        if (contentType is null)
        {
            return BadRequest(new
            {
                mensaje = "Solo se permiten archivos PDF, DOC y DOCX."
            });
        }

        await using var contenido =
            request.Archivo.OpenReadStream();

        try
        {
            var documentoId = await _mediator.Send(
                new CargarDocumentoCommand(
                    causaId,
                    request.Nombre,
                    request.Tipo,
                    request.Archivo.FileName,
                    request.Archivo.Length,
                    contenido),
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
                    mensaje = "Archivo cargado correctamente."
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
    [Authorize(Policy = "DocumentosLectura")]
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

    [HttpGet("Documentos/{id:guid}/archivo")]
    [Authorize(Policy = "DocumentosLectura")]
    public async Task<IActionResult> Descargar(
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

        var contenido = await _archivoStorage.AbrirLecturaAsync(
            documento.RutaArchivo,
            cancellationToken);

        if (contenido is null)
        {
        return NotFound(new
        {
            mensaje = "El archivo fisico no existe."
        });
    }

        var extension = Path.GetExtension(documento.RutaArchivo);

        var nombreDescarga = Path.HasExtension(documento.Nombre)
        ? documento.Nombre
        : documento.Nombre + extension;

        return File(
            contenido,
            documento.ContentType,
            nombreDescarga,
            enableRangeProcessing: true);
}
    [HttpGet("Causas/{causaId:guid}/documentos")]
    [Authorize(Policy = "DocumentosLectura")]
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
    [Authorize(Policy = "DocumentosEliminacion")]
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