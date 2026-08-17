using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CerrarOtrasSesiones;

public sealed record CerrarOtrasSesionesCommand(
    Guid UsuarioId,
    Guid SesionActualId,
    string? DireccionIp)
    : IRequest<int>;