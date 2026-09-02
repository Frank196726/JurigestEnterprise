using System.Text.Json;
using Jurigest.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.API.Controllers;

[ApiController]
[Route("api/operacion")]
[Authorize(Roles = "Administrador")]
public sealed class OperacionController(JurigestDbContext db) : ControllerBase
{
    [HttpGet("respaldo-judicial")]
    public async Task<IActionResult> DescargarRespaldo(CancellationToken cancellationToken)
    {
        var respaldo = new
        {
            version = 1,
            generadoEnUtc = DateTime.UtcNow,
            causas = await db.Causas.AsNoTracking().ToListAsync(cancellationToken),
            diligencias = await db.Diligencias.AsNoTracking().ToListAsync(cancellationToken),
            resoluciones = await db.Resoluciones.AsNoTracking().ToListAsync(cancellationToken),
            documentos = await db.Documentos.AsNoTracking().ToListAsync(cancellationToken)
        };
        var contenido = JsonSerializer.SerializeToUtf8Bytes(respaldo, new JsonSerializerOptions { WriteIndented = true });
        return File(contenido, "application/json", $"jurigest-respaldo-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
    }
}
