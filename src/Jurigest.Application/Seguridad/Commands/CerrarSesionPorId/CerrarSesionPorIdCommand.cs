using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CerrarSesionPorId;

public sealed record CerrarSesionPorIdCommand(
    Guid UsuarioId,
    Guid SesionId,
    string? DireccionIp)
    : IRequest<bool>;