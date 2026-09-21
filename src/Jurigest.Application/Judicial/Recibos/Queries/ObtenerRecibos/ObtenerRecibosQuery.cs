using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;

public sealed record ObtenerRecibosQuery
    : IRequest<List<ObtenerRecibosResponse>>;
