using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausa;

public sealed record ObtenerCausaQuery(Guid Id)
    : IRequest<ObtenerCausaResponse?>;