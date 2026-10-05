using Jurigest.Domain.Seguridad.Enums;

namespace Jurigest.API.Contracts;

public sealed class CrearUsuarioRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public Guid? RolCatalogoId { get; set; }

    public string Rut { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string NumeroOficina { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }
}
