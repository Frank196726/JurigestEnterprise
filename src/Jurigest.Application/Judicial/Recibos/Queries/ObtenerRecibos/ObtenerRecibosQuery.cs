using Jurigest.Application.Judicial;
using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;

public sealed record ObtenerRecibosQuery(FiltroReceptores? Receptores = null)
    : IRequest<List<ObtenerRecibosResponse>>;
