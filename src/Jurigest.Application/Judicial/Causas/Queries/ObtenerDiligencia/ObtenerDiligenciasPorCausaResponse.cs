using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorCausa;

public sealed record ObtenerDiligenciasPorCausaResponse(
    Guid Id,
    string Descripcion,
    EstadoDiligencia Estado,
    TipoDiligencia Tipo,
    ResultadoDiligencia Resultado,
    string? ResultadoDetalle,
    DateTime FechaCreacion,
    DateTime? FechaProgramada,
    DateTime? FechaRealizada,
    DateTime? FechaGestion,
    string? ReceptorJudicial,
    string? Direccion,
    string? Comuna,
    string? Estampe);