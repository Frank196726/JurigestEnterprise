using MediatR;

namespace Jurigest.Application.Seguridad.Queries.ObtenerSesionesUsuario;

public sealed record ObtenerSesionesUsuarioQuery(
    Guid UsuarioId,
    Guid SesionActualId)
    : IRequest<List<ObtenerSesionUsuarioResponse>>;