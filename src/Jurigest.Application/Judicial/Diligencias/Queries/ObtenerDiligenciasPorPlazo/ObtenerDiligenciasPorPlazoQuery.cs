using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorPlazo;

public sealed record ObtenerDiligenciasPorPlazoQuery(string? ReceptorAsignado = null)
    : IRequest<List<ObtenerDiligenciasPorPlazoResponse>>;
