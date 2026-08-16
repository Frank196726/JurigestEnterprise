using MediatR;

namespace Jurigest.Application.Seguridad.Commands.RenovarSesion;

public sealed record RenovarSesionCommand(
    string RefreshToken,
    string? DireccionIp,
    string? UserAgent)
    : IRequest<RenovarSesionResponse?>;

public sealed record RenovarSesionResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);