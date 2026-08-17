using MediatR;

namespace Jurigest.Application.Seguridad.Commands.ConfirmarRecuperacionPassword;

public sealed record ConfirmarRecuperacionPasswordCommand(
    string Token,
    string NuevaPassword,
    string? DireccionIp)
    : IRequest<ConfirmarRecuperacionPasswordResultado>;

public enum ConfirmarRecuperacionPasswordResultado
{
    Completada = 1,
    TokenInvalido = 2
}