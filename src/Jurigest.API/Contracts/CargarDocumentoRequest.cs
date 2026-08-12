using Jurigest.Domain.Judicial.Enums;
using Microsoft.AspNetCore.Http;

namespace Jurigest.API.Contracts;

public sealed class CargarDocumentoRequest
{
    public string Nombre { get; set; } = string.Empty;

    public TipoDocumento Tipo { get; set; }

    public IFormFile? Archivo { get; set; }
}