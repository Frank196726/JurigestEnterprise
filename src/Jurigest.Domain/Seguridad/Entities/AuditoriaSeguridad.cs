using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Seguridad.Entities;

public sealed class AuditoriaSeguridad : Entity<Guid>
{
    private AuditoriaSeguridad()
    {
    }

    public AuditoriaSeguridad(
        Guid id,
        Guid? usuarioActorId,
        string accion,
        Guid? usuarioAfectadoId,
        string? detalle,
        string? direccionIp)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accion);

        UsuarioActorId = usuarioActorId;
        Accion = accion.Trim();
        UsuarioAfectadoId = usuarioAfectadoId;
        Detalle = NormalizarOpcional(detalle);
        DireccionIp = NormalizarOpcional(direccionIp);
        FechaUtc = DateTime.UtcNow;
    }

    public Guid? UsuarioActorId { get; private set; }

    public string Accion { get; private set; } = string.Empty;

    public Guid? UsuarioAfectadoId { get; private set; }

    public string? Detalle { get; private set; }

    public string? DireccionIp { get; private set; }

    public DateTime FechaUtc { get; private set; }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim();
    }
}