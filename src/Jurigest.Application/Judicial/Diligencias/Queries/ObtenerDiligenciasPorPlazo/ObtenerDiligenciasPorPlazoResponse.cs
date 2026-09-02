using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorPlazo;

public sealed record ObtenerDiligenciasPorPlazoResponse(
    Guid Id,
    Guid CausaId,
    string Rit,
    string Tribunal,
    string Descripcion,
    EstadoDiligencia Estado,
    TipoDiligencia Tipo,
    DateTime FechaProgramada,
    int DiasParaVencimiento,
    string? ReceptorJudicial);
