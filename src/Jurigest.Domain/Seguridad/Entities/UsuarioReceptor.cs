namespace Jurigest.Domain.Seguridad.Entities;

/// <summary>
/// Asociación de un usuario con un receptor judicial. Un usuario puede
/// estar asociado a varios receptores.
/// </summary>
public sealed class UsuarioReceptor
{
    private UsuarioReceptor()
    {
    }

    public UsuarioReceptor(Guid usuarioId, Guid receptorJudicialId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuario es obligatorio.", nameof(usuarioId));

        if (receptorJudicialId == Guid.Empty)
            throw new ArgumentException("El receptor es obligatorio.", nameof(receptorJudicialId));

        UsuarioId = usuarioId;
        ReceptorJudicialId = receptorJudicialId;
        FechaAsignacion = DateTime.UtcNow;
    }

    public Guid UsuarioId { get; private set; }

    public Guid ReceptorJudicialId { get; private set; }

    public DateTime FechaAsignacion { get; private set; }
}
