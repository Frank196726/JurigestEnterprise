using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class CambiarTipoDiligenciaRequest
{
    public TipoDiligencia Tipo { get; set; }
}
