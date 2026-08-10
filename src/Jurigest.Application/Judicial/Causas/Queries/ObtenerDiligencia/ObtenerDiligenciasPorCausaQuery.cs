using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligenciasPorCausa;

public sealed record ObtenerDiligenciasPorCausaQuery(
    Guid CausaId)
    : IRequest<List<ObtenerDiligenciasPorCausaResponse>>;