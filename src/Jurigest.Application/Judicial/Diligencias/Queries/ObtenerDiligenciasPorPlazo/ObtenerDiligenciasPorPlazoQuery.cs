using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorPlazo;

public sealed record ObtenerDiligenciasPorPlazoQuery
    : IRequest<List<ObtenerDiligenciasPorPlazoResponse>>;
