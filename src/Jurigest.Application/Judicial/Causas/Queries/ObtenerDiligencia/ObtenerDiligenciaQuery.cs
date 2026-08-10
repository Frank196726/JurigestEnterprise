using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Queries.ObtenerDiligencia;

public sealed record ObtenerDiligenciaQuery(Guid Id)
    : IRequest<ObtenerDiligenciaResponse?>;