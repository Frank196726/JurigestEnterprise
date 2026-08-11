using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class RegistrarDocumentoRequest
{
    public string Nombre { get; set; } = string.Empty;

    public TipoDocumento Tipo { get; set; }

    public string RutaArchivo { get; set; } = string.Empty;
}