using Jurigest.Domain.Kernel.Common;
using Jurigest.Domain.Seguridad.Enums;

namespace Jurigest.Domain.Seguridad.Entities;

public sealed class Usuario : Entity<Guid>
{
    private Usuario()
    {
    }

    public Usuario(
        Guid id,
        string nombre,
        string email,
        string passwordHash,
        RolUsuario rol)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        if (!Enum.IsDefined(rol))
            throw new ArgumentException("El rol no es valido.");

        Nombre = nombre.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true;
        VersionSeguridad = 1;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public RolUsuario Rol { get; private set; }

    public bool Activo { get; private set; }

    public int VersionSeguridad { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        InvalidarSesiones();
    }

    public void CambiarRol(RolUsuario rol)
    {
        if (!Enum.IsDefined(rol))
            throw new ArgumentException("El rol no es valido.");

        if (Rol == rol)
            return;

        Rol = rol;
        InvalidarSesiones();
    }

    public void Desactivar()
    {
        if (!Activo)
            return;

        Activo = false;
        InvalidarSesiones();
    }

    public void Activar()
    {
        Activo = true;
    }

    private void InvalidarSesiones()
    {
        VersionSeguridad = checked(VersionSeguridad + 1);
    }
}