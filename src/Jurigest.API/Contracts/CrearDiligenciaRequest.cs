using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class CrearDiligenciaRequest
{
    public string Descripcion { get; set; } = string.Empty;

    public TipoDiligencia Tipo { get; set; } =
        TipoDiligencia.Otro;
}
