using Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibos;
using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Queries.ObtenerRecibo;

public sealed record ObtenerReciboQuery(Guid Id)
    : IRequest<ObtenerRecibosResponse?>;
