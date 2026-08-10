using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorCausa;

public sealed record ObtenerDiligenciasPorCausaResponse(
    Guid Id,
    string Descripcion,
    EstadoDiligencia Estado,
    TipoDiligencia Tipo,
    DateTime FechaCreacion,
    DateTime? FechaProgramada,
    DateTime? FechaRealizada,
    string? ReceptorJudicial,
    string? Direccion,
    string? Comuna);