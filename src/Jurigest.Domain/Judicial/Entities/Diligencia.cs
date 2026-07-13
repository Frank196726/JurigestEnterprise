using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

/// <summary>
/// Representa una diligencia realizada dentro de un encargo.
/// </summary>
public sealed class Diligencia : Entity<Guid>
{
    private Diligencia()
    {
    }

    public Diligencia(Guid id, string descripcion)
        : base(id)
    {
        Descripcion = descripcion;
        Fecha = DateTime.UtcNow;
    }

    public string Descripcion { get; private set; }

    public DateTime Fecha { get; private set; }

    public bool Completada { get; private set; }

    public void Completar()
    {
        Completada = true;
    }
}