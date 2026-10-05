using Jurigest.Domain.Kernel.Common;
using Jurigest.Domain.Judicial;
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
        DebeCambiarPassword = false;
        Activo = true;
        VersionSeguridad = 1;
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Rut { get; private set; }
    public string? Telefono { get; private set; }
    public string? Direccion { get; private set; }
    public string? NumeroOficina { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public RolUsuario Rol { get; private set; }

    public Guid? RolCatalogoId { get; private set; }
    public string? NombreRolPersonalizado { get; private set; }

    public bool DebeCambiarPassword { get; private set; }

    public void AsignarRolCatalogo(RolCatalogo rol)
    {
        ArgumentNullException.ThrowIfNull(rol);
        CambiarRol(rol.Perfil);
        RolCatalogoId = rol.Id;
        NombreRolPersonalizado = rol.Nombre;
    }

    public bool Activo { get; private set; }

    public int VersionSeguridad { get; private set; }

    public int IntentosFallidos { get; private set; }

    public DateTime? BloqueadoHastaUtc { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public void CambiarPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        DebeCambiarPassword = false;
        RestablecerIntentosFallidos();
        InvalidarSesiones();
    }

    public void RequerirCambioPassword() => DebeCambiarPassword = true;

    public void ActualizarDatosPersonales(
        string rut,
        string telefono,
        string direccion,
        string numeroOficina)
    {
        Rut = RutChileno.Normalizar(rut)
            ?? throw new ArgumentException("El RUT es obligatorio.", nameof(rut));
        Telefono = NormalizarObligatorio(telefono, 30, "El teléfono es obligatorio.");
        Direccion = NormalizarObligatorio(direccion, 250, "La dirección es obligatoria.");
        NumeroOficina = NormalizarObligatorio(numeroOficina, 50, "El N° de oficina es obligatorio.");
    }

    public void ActualizarIdentidad(string nombre, string email)
    {
        var nuevoNombre = NormalizarObligatorio(nombre, 200, "El nombre es obligatorio.");
        var correo = NormalizarObligatorio(email, 200, "El correo es obligatorio.").ToLowerInvariant();
        if (!System.Net.Mail.MailAddress.TryCreate(correo, out var direccion) || direccion.Address != correo)
            throw new ArgumentException("El correo no es válido.");
        if (Nombre != nuevoNombre || Email != correo) InvalidarSesiones();
        Nombre = nuevoNombre;
        Email = correo;
    }

    public void CambiarRol(RolUsuario rol)
    {
        if (!Enum.IsDefined(rol))
            throw new ArgumentException("El rol no es valido.");

        if (Rol == rol && RolCatalogoId is null)
            return;

        RolCatalogoId = null;
        NombreRolPersonalizado = null;
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

    private static string NormalizarObligatorio(string valor, int maximo, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException(mensaje);

        var normalizado = valor.Trim();
        if (normalizado.Length > maximo)
            throw new ArgumentException($"El valor no puede superar {maximo} caracteres.");

        return normalizado;
    }
}
