using Jurigest.Application.Judicial;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorPlazo;

public sealed record ObtenerDiligenciasPorPlazoQuery(FiltroReceptores? Receptores = null)
    : IRequest<List<ObtenerDiligenciasPorPlazoResponse>>;
