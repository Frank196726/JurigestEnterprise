using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligencia;

public sealed record ObtenerDiligenciaResponse(
    Guid Id,
    Guid CausaId,
    string Descripcion,
    TipoDiligencia Tipo,
    EstadoDiligencia Estado,
    DateTime FechaCreacion,
    DateTime? FechaProgramada,
    DateTime? FechaRealizada,
    string? ReceptorJudicial,
    string? Direccion,
    string? Comuna,
    string? Observaciones,
    decimal? Latitud,
    decimal? Longitud);