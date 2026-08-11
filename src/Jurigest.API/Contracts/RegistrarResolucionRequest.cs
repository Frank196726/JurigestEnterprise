using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class RegistrarResolucionRequest
{
    public TipoResolucion Tipo { get; set; }

    public DateTime Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;
}