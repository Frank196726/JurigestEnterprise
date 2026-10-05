using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausas;

public sealed record ObtenerCausasQuery(string? ReceptorAsignado = null)
    : IRequest<List<ObtenerCausasResponse>>;
