using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class CargarDocumentoRequest
{
    public string Nombre { get; set; } = string.Empty;

    public TipoDocumento Tipo { get; set; }

    public IFormFile? Archivo { get; set; }
}