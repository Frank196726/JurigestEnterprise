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
    : IRequest<CrearUsuarioResult>
{
    public Guid? RolCatalogoId { get; init; }
    public string Rut { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public string NumeroOficina { get; init; } = string.Empty;
}

public sealed record CrearUsuarioResult(
    bool Creado,
    bool EmailDuplicado,
    bool RutDuplicado,
    Guid? UsuarioId);
