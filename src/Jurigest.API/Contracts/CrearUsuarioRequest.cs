using Jurigest.Domain.Seguridad.Enums;

namespace Jurigest.API.Contracts;

public sealed class CrearUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }
}