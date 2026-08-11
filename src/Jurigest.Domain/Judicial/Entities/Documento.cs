using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class Documento : Entity<Guid>
{
    private Documento()
    {
    }

    public Documento(
        Guid id,
        Guid causaId,
        string nombre,
        TipoDocumento tipo,
        string rutaArchivo)
        : base(id)
    {
        if (causaId == Guid.Empty)
            throw new ArgumentException("La causa es obligatoria.");

        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);

        CausaId = causaId;
        Nombre = nombre.Trim();
        Tipo = tipo;
        RutaArchivo = rutaArchivo.Trim();
        FechaRegistro = DateTime.UtcNow;
    }

    public Guid CausaId { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public TipoDocumento Tipo { get; private set; }

    public string RutaArchivo { get; private set; } = string.Empty;

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