using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CambiarPasswordInicial;

public sealed record CambiarPasswordInicialCommand(
    Guid UsuarioId,
    string NuevaPassword,
    string? DireccionIp)
    : IRequest<bool>;
