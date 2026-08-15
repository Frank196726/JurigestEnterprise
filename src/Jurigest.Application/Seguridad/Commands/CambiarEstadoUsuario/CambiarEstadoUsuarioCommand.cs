using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CambiarEstadoUsuario;

public sealed record CambiarEstadoUsuarioCommand(
    Guid UsuarioId,
    bool Activo,
    Guid AdministradorId)
    : IRequest<CambiarEstadoUsuarioResultado>;