using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Queries.ObtenerResolucion;

public sealed record ObtenerResolucionQuery(Guid Id)
    : IRequest<ObtenerResolucionResponse?>;