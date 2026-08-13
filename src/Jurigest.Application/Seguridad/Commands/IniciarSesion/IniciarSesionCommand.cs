using Jurigest.Application.Abstractions.Security;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.IniciarSesion;

public sealed record IniciarSesionCommand(
    string Email,
    string Password)
    : IRequest<IniciarSesionResponse?>;

public sealed record IniciarSesionResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UsuarioId,
    string Nombre,
    string Email,
    string Rol);