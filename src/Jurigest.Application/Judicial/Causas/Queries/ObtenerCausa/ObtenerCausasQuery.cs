using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausas;

public sealed record ObtenerCausasQuery()
    : IRequest<List<ObtenerCausasResponse>>;