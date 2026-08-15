using Jurigest.Application.Seguridad.Queries.ObtenerUsuarios;
using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerUsuario;

public sealed record ObtenerUsuarioQuery(Guid Id)
    : IRequest<ObtenerUsuariosResponse?>;