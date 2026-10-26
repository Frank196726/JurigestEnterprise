using Jurigest.Application.Contabilidad;
using Jurigest.Domain.Contabilidad.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/contabilidad")]
public sealed class ContabilidadController : ControllerBase
{
    private readonly ContabilidadApplicationService _service;

    public ContabilidadController(ContabilidadApplicationService service)
    {
        _service = service;
    }

    [HttpGet("cuentas")]
    public async Task<ActionResult<IEnumerable<CuentaDto>>> GetCuentas(CancellationToken cancellationToken)
    {
        var cuentas = await _service.ObtenerCuentasAsync(cancellationToken);
        return Ok(cuentas.Select(c => new CuentaDto(c.Id, c.Codigo, c.Nombre, c.Saldo)));
    }

    [HttpGet("cuentas/{id:guid}")]
    public async Task<ActionResult<CuentaDto>> GetCuenta(Guid id, CancellationToken cancellationToken)
    {
        var cuenta = await _service.ObtenerCuentaAsync(id, cancellationToken);
        if (cuenta is null)
            return NotFound();

        return Ok(new CuentaDto(cuenta.Id, cuenta.Codigo, cuenta.Nombre, cuenta.Saldo));
    }

    [HttpPost("cuentas")]
    public async Task<ActionResult<CuentaDto>> CrearCuenta([FromBody] CrearCuentaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Codigo) || string.IsNullOrWhiteSpace(request.Nombre))
            return ValidationProblem();

        var cuenta = await _service.CrearCuentaAsync(request.Codigo, request.Nombre, request.SaldoInicial, cancellationToken);
        return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.Id }, new CuentaDto(cuenta.Id, cuenta.Codigo, cuenta.Nombre, cuenta.Saldo));
    }

    [HttpPost("cuentas/{id:guid}/movimientos")]
    public async Task<ActionResult<CuentaDto>> RegistrarMovimiento(Guid id, [FromBody] RegistrarMovimientoRequest request, CancellationToken cancellationToken)
    {
        var cuenta = await _service.RegistrarMovimientoAsync(id, request.Monto, request.Descripcion, request.Tipo, cancellationToken);
        return Ok(new CuentaDto(cuenta.Id, cuenta.Codigo, cuenta.Nombre, cuenta.Saldo));
    }

    [HttpGet("asientos")]
    public async Task<ActionResult<IEnumerable<AsientoDto>>> GetAsientos(CancellationToken cancellationToken)
    {
        var asientos = await _service.ObtenerAsientosAsync(cancellationToken);
        return Ok(asientos.Select(a => new AsientoDto(a.Id, a.Descripcion, a.Fecha, a.Cuadrado, a.TotalDebe, a.TotalHaber)));
    }

    [HttpPost("asientos")]
    public async Task<ActionResult<AsientoDto>> CrearAsiento([FromBody] CrearAsientoRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            return ValidationProblem();

        var asiento = await _service.CrearAsientoAsync(request.Descripcion, cancellationToken);
        return CreatedAtAction(nameof(GetAsientos), null, new AsientoDto(asiento.Id, asiento.Descripcion, asiento.Fecha, asiento.Cuadrado, asiento.TotalDebe, asiento.TotalHaber));
    }

    [HttpPost("asientos/{id:guid}/movimientos")]
    public async Task<ActionResult<AsientoDto>> AgregarMovimientoAsiento(Guid id, [FromBody] RegistrarMovimientoRequest request, CancellationToken cancellationToken)
    {
        var asiento = await _service.AgregarMovimientoAsientoAsync(id, request.CuentaId, request.Monto, request.Descripcion, request.Tipo, cancellationToken);
        return Ok(new AsientoDto(asiento.Id, asiento.Descripcion, asiento.Fecha, asiento.Cuadrado, asiento.TotalDebe, asiento.TotalHaber));
    }
}

public sealed record CrearCuentaRequest(string Codigo, string Nombre, decimal SaldoInicial = 0m);
public sealed record RegistrarMovimientoRequest(Guid CuentaId, decimal Monto, string Descripcion, TipoMovimiento Tipo);
public sealed record CrearAsientoRequest(string Descripcion);
public sealed record CuentaDto(Guid Id, string Codigo, string Nombre, decimal Saldo);
public sealed record AsientoDto(Guid Id, string Descripcion, DateTime Fecha, bool Cuadrado, decimal TotalDebe, decimal TotalHaber);
