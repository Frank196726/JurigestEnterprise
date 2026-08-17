using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Seguridad.Entities;

public sealed class TokenRecuperacionPassword
    : Entity<Guid>
{
    private TokenRecuperacionPassword()
    {
    }

    public TokenRecuperacionPassword(
        Guid id,
        Guid usuarioId,
        string tokenHash,
        DateTime fechaCreacionUtc,
        DateTime expiraUtc)
        : base(id)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException(
                "El usuario es obligatorio.",
                nameof(usuarioId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            tokenHash);

        if (expiraUtc <= fechaCreacionUtc)
        {
            throw new ArgumentException(
                "La expiracion debe ser posterior a la creacion.",
                nameof(expiraUtc));
        }

        UsuarioId = usuarioId;
        TokenHash = tokenHash.Trim();
        FechaCreacionUtc = fechaCreacionUtc;
        ExpiraUtc = expiraUtc;
    }

    public Guid UsuarioId { get; private set; }

    public string TokenHash { get; private set; } =
        string.Empty;

    public DateTime FechaCreacionUtc { get; private set; }

    public DateTime ExpiraUtc { get; private set; }

    public DateTime? UsadoUtc { get; private set; }

    public DateTime? RevocadoUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public bool EstaDisponible(DateTime fechaUtc)
    {
        return UsadoUtc is null &&
            RevocadoUtc is null &&
            ExpiraUtc > fechaUtc;
    }

    public void MarcarUsado(DateTime fechaUtc)
    {
        if (!EstaDisponible(fechaUtc))
        {
            throw new InvalidOperationException(
                "El token de recuperacion no esta disponible.");
        }

        UsadoUtc = fechaUtc;
    }

    public void Revocar(DateTime fechaUtc)
    {
        if (UsadoUtc.HasValue ||
            RevocadoUtc.HasValue)
        {
            return;
        }

        RevocadoUtc = fechaUtc;
    }
}