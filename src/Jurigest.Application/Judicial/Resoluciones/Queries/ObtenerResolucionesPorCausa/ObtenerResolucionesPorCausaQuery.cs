using Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucion;
using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucionesPorCausa;

public sealed record ObtenerResolucionesPorCausaQuery(Guid CausaId)
    : IRequest<List<ObtenerResolucionResponse>>;