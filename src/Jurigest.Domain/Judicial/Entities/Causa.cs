using Jurigest.Domain.Kernel.Common;
using Jurigest.Domain.Judicial.ValueObjects;

namespace Jurigest.Domain.Judicial.Entities;

/// <summary>
/// Representa una causa judicial.
/// </summary>
public sealed class Causa : AggregateRoot<Guid>
{
    private Causa()
    {
    }

    public Causa(Guid id, RolUnicoTribunal rit)
        : base(id)
    {
        Rit = rit;
    }

    /// <summary>
    /// Rol Único del Tribunal (RIT).
    /// </summary>
    public RolUnicoTribunal Rit { get; private set; }
    public Encargo? Encargo { get; private set; }

public void AsignarEncargo(Encargo encargo)
{
    ArgumentNullException.ThrowIfNull(encargo);

    if (Encargo is not null)
        throw new InvalidOperationException("La causa ya posee un encargo.");

    Encargo = encargo;
}
    
}