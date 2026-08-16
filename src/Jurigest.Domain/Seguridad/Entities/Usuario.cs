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
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public RolUsuario Rol { get; private set; }

    public bool Activo { get; private set; }

    public int VersionSeguridad { get; private set; }

    public int IntentosFallidos { get; private set; }

    public DateTime? BloqueadoHastaUtc { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        RestablecerIntentosFallidos();
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

    public bool EstaBloqueado(DateTime fechaUtc)
    {
        return BloqueadoHastaUtc.HasValue &&
            BloqueadoHastaUtc.Value > fechaUtc;
    }

    public bool RegistrarIntentoFallido(
        DateTime fechaUtc,
        int maximoIntentos,
        TimeSpan duracionBloqueo)
    {
        if (maximoIntentos <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximoIntentos));
        }

        if (duracionBloqueo <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duracionBloqueo));
        }

        if (BloqueadoHastaUtc.HasValue &&
            BloqueadoHastaUtc.Value <= fechaUtc)
        {
            IntentosFallidos = 0;
            BloqueadoHastaUtc = null;
        }

        IntentosFallidos =
            checked(IntentosFallidos + 1);

        if (IntentosFallidos < maximoIntentos)
            return false;

        IntentosFallidos = 0;
        BloqueadoHastaUtc =
            fechaUtc.Add(duracionBloqueo);

        return true;
    }

    public void RestablecerIntentosFallidos()
    {
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
    }

    public void InvalidarSesiones()
    {
        VersionSeguridad =
            checked(VersionSeguridad + 1);
    }
}