using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerUsuarios;

public sealed record ObtenerUsuariosQuery
    : IRequest<List<ObtenerUsuariosResponse>>;