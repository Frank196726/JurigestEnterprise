using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

/// <summary>
/// Representa el encargo profesional asociado a una causa.
/// </summary>
public sealed class Encargo : AggregateRoot<Guid>
{
    private readonly List<Diligencia> _diligencias = [];
    private readonly List<Documento> _documentos = [];

    private Encargo()
    {
    }

    public Encargo(Guid id, string descripcion)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        Descripcion = descripcion.Trim();
        Estado = EstadoEncargo.Creado;
    }

    public string Descripcion { get; private set; }

    public EstadoEncargo Estado { get; private set; }

    public IReadOnlyCollection<Diligencia> Diligencias
        => _diligencias.AsReadOnly();

    public IReadOnlyCollection<Documento> Documentos
        => _documentos.AsReadOnly();

    public void AgregarDiligencia(Diligencia diligencia)
    {
        ArgumentNullException.ThrowIfNull(diligencia);

        _diligencias.Add(diligencia);
    }

    public void AgregarDocumento(Documento documento)
    {
        ArgumentNullException.ThrowIfNull(documento);

        if (_documentos.Any(d => d.Id == documento.Id))
            throw new InvalidOperationException(
                "El documento ya existe en el encargo.");

        _documentos.Add(documento);
    }

    public void CambiarEstado(EstadoEncargo nuevoEstado)
    {
        Estado = nuevoEstado;
    }
}