using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

/// <summary>
/// Representa el encargo profesional asociado a una causa.
/// </summary>
public sealed class Encargo : AggregateRoot<Guid>
{
    private readonly List<Diligencia> _diligencias = [];

    private Encargo()
    {
    }

    public Encargo(Guid id, string descripcion)
        : base(id)
    {
        Descripcion = descripcion;
        Estado = EstadoEncargo.Creado;
    }

    public string Descripcion { get; private set; }

    public EstadoEncargo Estado { get; private set; }

    public IReadOnlyCollection<Diligencia> Diligencias
        => _diligencias.AsReadOnly();

    public void AgregarDiligencia(Diligencia diligencia)
    {
        ArgumentNullException.ThrowIfNull(diligencia);

        _diligencias.Add(diligencia);
    }

    public void CambiarEstado(EstadoEncargo nuevoEstado)
    {
        Estado = nuevoEstado;
    }
}