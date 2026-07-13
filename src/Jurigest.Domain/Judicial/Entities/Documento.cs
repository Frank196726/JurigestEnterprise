using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

/// <summary>
/// Representa un documento asociado a un encargo.
/// </summary>
public sealed class Documento : Entity<Guid>
{
    private Documento()
    {
    }

    public Documento(
        Guid id,
        string nombre,
        TipoDocumento tipo,
        string rutaArchivo)
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        Nombre = nombre.Trim();
        Tipo = tipo;
        RutaArchivo = rutaArchivo.Trim();
        FechaRegistro = DateTime.UtcNow;
    }

    public string Nombre { get; private set; }

    public TipoDocumento Tipo { get; private set; }

    public string RutaArchivo { get; private set; }

    public DateTime FechaRegistro { get; private set; }

    public void CambiarNombre(string nuevoNombre)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nuevoNombre);

        Nombre = nuevoNombre.Trim();
    }

    public void CambiarRuta(string nuevaRuta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nuevaRuta);

        RutaArchivo = nuevaRuta.Trim();
    }
}