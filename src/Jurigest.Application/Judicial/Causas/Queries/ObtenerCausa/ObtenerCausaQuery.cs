using Jurigest.Application.Judicial;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.ObtenerCausa;

public sealed record ObtenerCausaQuery(Guid Id, FiltroReceptores? Receptores = null)
    : IRequest<ObtenerCausaResponse?>;
