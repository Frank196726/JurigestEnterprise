using Jurigest.Application.Abstractions.Security;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.IniciarSesion;

public sealed record IniciarSesionCommand(
    string Email,
    string Password,
    string? DireccionIp,
    string? UserAgent)
    : IRequest<IniciarSesionResponse?>;

public sealed record IniciarSesionResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    Guid UsuarioId,
    string Nombre,
    string Email,
    string Rol);