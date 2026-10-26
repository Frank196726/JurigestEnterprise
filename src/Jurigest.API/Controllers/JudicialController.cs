using Jurigest.Application.Judicial;
using Jurigest.Domain.Judicial.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/judicial")]
public sealed class JudicialController : ControllerBase
{
    private readonly JudicialApplicationService _service;

    public JudicialController(JudicialApplicationService service)
    {
        _service = service;
    }

    [HttpGet("causas")]
    public async Task<ActionResult<IEnumerable<CausaDto>>> GetCausas(CancellationToken cancellationToken)
    {
        var causas = await _service.ObtenerCausasAsync(cancellationToken);
        return Ok(causas.Select(c => new CausaDto(c.Id, c.Rit.Valor, c.Encargo is null ? null : c.Encargo.Id)));
    }

    [HttpGet("causas/{id:guid}")]
    public async Task<ActionResult<CausaDto>> GetCausa(Guid id, CancellationToken cancellationToken)
    {
        var causa = await _service.ObtenerCausaAsync(id, cancellationToken);
        if (causa is null)
            return NotFound();

        return Ok(new CausaDto(causa.Id, causa.Rit.Valor, causa.Encargo is null ? null : causa.Encargo.Id));
    }

    [HttpPost("causas")]
    public async Task<ActionResult<CausaDto>> CrearCausa([FromBody] CrearCausaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rit))
            return ValidationProblem();

        var causa = await _service.CrearCausaAsync(request.Rit, cancellationToken);
        return CreatedAtAction(nameof(GetCausa), new { id = causa.Id }, new CausaDto(causa.Id, causa.Rit.Valor, causa.Encargo is null ? null : causa.Encargo.Id));
    }

    [HttpPost("causas/{id:guid}/encargo")]
    public async Task<ActionResult<CausaDto>> AsignarEncargo(Guid id, [FromBody] AsignarEncargoRequest request, CancellationToken cancellationToken)
    {
        var causa = await _service.AsignarEncargoAsync(id, request.EncargoId, cancellationToken);
        return Ok(new CausaDto(causa.Id, causa.Rit.Valor, causa.Encargo is null ? null : causa.Encargo.Id));
    }

    [HttpGet("encargos")]
    public async Task<ActionResult<IEnumerable<EncargoDto>>> GetEncargos(CancellationToken cancellationToken)
    {
        var encargos = await _service.ObtenerEncargosAsync(cancellationToken);
        return Ok(encargos.Select(e => new EncargoDto(e.Id, e.Descripcion, e.Estado, e.Diligencias.Count, e.Documentos.Count, e.Resoluciones.Count)));
    }

    [HttpGet("encargos/{id:guid}")]
    public async Task<ActionResult<EncargoDto>> GetEncargo(Guid id, CancellationToken cancellationToken)
    {
        var encargo = await _service.ObtenerEncargoAsync(id, cancellationToken);
        if (encargo is null)
            return NotFound();

        return Ok(new EncargoDto(encargo.Id, encargo.Descripcion, encargo.Estado, encargo.Diligencias.Count, encargo.Documentos.Count, encargo.Resoluciones.Count));
    }

    [HttpPost("encargos")]
    public async Task<ActionResult<EncargoDto>> CrearEncargo([FromBody] CrearEncargoRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            return ValidationProblem();

        var encargo = await _service.CrearEncargoAsync(request.Descripcion, cancellationToken);
        return CreatedAtAction(nameof(GetEncargo), new { id = encargo.Id }, new EncargoDto(encargo.Id, encargo.Descripcion, encargo.Estado, encargo.Diligencias.Count, encargo.Documentos.Count, encargo.Resoluciones.Count));
    }

    [HttpPost("encargos/{id:guid}/diligencias")]
    public async Task<ActionResult<EncargoDto>> AgregarDiligencia(Guid id, [FromBody] AgregarDiligenciaRequest request, CancellationToken cancellationToken)
    {
        var encargo = await _service.AgregarDiligenciaAsync(id, request.Descripcion, cancellationToken);
        return Ok(new EncargoDto(encargo.Id, encargo.Descripcion, encargo.Estado, encargo.Diligencias.Count, encargo.Documentos.Count, encargo.Resoluciones.Count));
    }

    [HttpPost("encargos/{id:guid}/documentos")]
    public async Task<ActionResult<EncargoDto>> AgregarDocumento(Guid id, [FromBody] AgregarDocumentoRequest request, CancellationToken cancellationToken)
    {
        var encargo = await _service.AgregarDocumentoAsync(id, request.Nombre, request.Tipo, request.RutaArchivo, cancellationToken);
        return Ok(new EncargoDto(encargo.Id, encargo.Descripcion, encargo.Estado, encargo.Diligencias.Count, encargo.Documentos.Count, encargo.Resoluciones.Count));
    }

    [HttpPost("encargos/{id:guid}/resoluciones")]
    public async Task<ActionResult<EncargoDto>> RegistrarResolucion(Guid id, [FromBody] RegistrarResolucionRequest request, CancellationToken cancellationToken)
    {
        var encargo = await _service.RegistrarResolucionAsync(id, request.Tipo, request.Fecha, request.Descripcion, cancellationToken);
        return Ok(new EncargoDto(encargo.Id, encargo.Descripcion, encargo.Estado, encargo.Diligencias.Count, encargo.Documentos.Count, encargo.Resoluciones.Count));
    }
}

public sealed record CrearCausaRequest(string Rit);
public sealed record AsignarEncargoRequest(Guid EncargoId);
public sealed record CrearEncargoRequest(string Descripcion);
public sealed record AgregarDiligenciaRequest(string Descripcion);
public sealed record AgregarDocumentoRequest(string Nombre, TipoDocumento Tipo, string RutaArchivo);
public sealed record RegistrarResolucionRequest(TipoResolucion Tipo, DateTime Fecha, string Descripcion);
public sealed record CausaDto(Guid Id, string Rit, Guid? EncargoId);
public sealed record EncargoDto(Guid Id, string Descripcion, EstadoEncargo Estado, int Diligencias, int Documentos, int Resoluciones);
