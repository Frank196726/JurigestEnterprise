using Jurigest.Application.Judicial;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausas;

public sealed record ObtenerCausasQuery(FiltroReceptores? Receptores = null)
    : IRequest<List<ObtenerCausasResponse>>;
