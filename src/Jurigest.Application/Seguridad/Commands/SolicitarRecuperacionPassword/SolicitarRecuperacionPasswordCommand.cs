using MediatR;

namespace Jurigest.Application.Seguridad.Commands.SolicitarRecuperacionPassword;

public sealed record SolicitarRecuperacionPasswordCommand(
    string Email,
    string? DireccionIp)
    : IRequest<SolicitarRecuperacionPasswordResponse?>;

public sealed record SolicitarRecuperacionPasswordResponse(
    string Token,
    DateTime ExpiraUtc);