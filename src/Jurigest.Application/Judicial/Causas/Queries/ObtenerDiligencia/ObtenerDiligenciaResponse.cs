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
    decimal? Longitud)
{
    public ResultadoDiligencia Resultado { get; init; }
    public string? ResultadoDetalle { get; init; }
    public Guid? DiligenciaRealizadaId { get; init; }
    public string? DiligenciaRealizada { get; init; }
    public DateTime? FechaGestion { get; init; }
    public string? Estampe { get; init; }
}