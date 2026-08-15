using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerAuditoriasSeguridad;

public sealed record ObtenerAuditoriasSeguridadQuery(
    int Cantidad)
    : IRequest<List<ObtenerAuditoriaSeguridadResponse>>;