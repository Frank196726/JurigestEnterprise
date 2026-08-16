using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Seguridad.Entities;

public sealed class SesionUsuario : Entity<Guid>
{
    private SesionUsuario()
    {
    }

    public SesionUsuario(
        Guid id,
        Guid usuarioId,
        string refreshTokenHash,
        int versionSeguridad,
        DateTime fechaCreacionUtc,
        DateTime expiraUtc,
        string? direccionIp,
        string? userAgent)
        : base(id)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException(
                "El usuario es obligatorio.",
                nameof(usuarioId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            refreshTokenHash);

        if (versionSeguridad <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(versionSeguridad));
        }

        if (expiraUtc <= fechaCreacionUtc)
        {
            throw new ArgumentException(
                "La expiracion debe ser posterior a la creacion.",
                nameof(expiraUtc));
        }

        UsuarioId = usuarioId;
        RefreshTokenHash = refreshTokenHash;
        VersionSeguridad = versionSeguridad;
        FechaCreacionUtc = fechaCreacionUtc;
        ExpiraUtc = expiraUtc;
        DireccionIp = NormalizarOpcional(direccionIp);
        UserAgent = NormalizarOpcional(userAgent);
    }

    public Guid UsuarioId { get; private set; }

    public string RefreshTokenHash { get; private set; } =
        string.Empty;

    public int VersionSeguridad { get; private set; }

    public DateTime FechaCreacionUtc { get; private set; }

    public DateTime ExpiraUtc { get; private set; }

    public DateTime? RevocadaUtc { get; private set; }

    public Guid? ReemplazadaPorId { get; private set; }

    public string? DireccionIp { get; private set; }

    public string? UserAgent { get; private set; }

    public bool EstaActiva(DateTime fechaUtc)
    {
        return RevocadaUtc is null &&
            ExpiraUtc > fechaUtc;
    }

    public void Revocar(
        DateTime fechaUtc,
        Guid? reemplazadaPorId = null)
    {
        if (RevocadaUtc.HasValue)
            return;

        if (reemplazadaPorId == Id)
        {
            throw new ArgumentException(
                "Una sesion no puede reemplazarse a si misma.",
                nameof(reemplazadaPorId));
        }

        RevocadaUtc = fechaUtc;
        ReemplazadaPorId = reemplazadaPorId;
    }

    private static string? NormalizarOpcional(
        string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}