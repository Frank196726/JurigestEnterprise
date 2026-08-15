using Jurigest.Domain.Seguridad.Enums;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CrearUsuario;

public sealed record CrearUsuarioCommand(
    string Nombre,
    string Email,
    string Password,
    RolUsuario Rol,
    Guid UsuarioActorId,
    string? DireccionIp)
    : IRequest<CrearUsuarioResult>;

public sealed record CrearUsuarioResult(
    bool Creado,
    bool EmailDuplicado,
    Guid? UsuarioId);