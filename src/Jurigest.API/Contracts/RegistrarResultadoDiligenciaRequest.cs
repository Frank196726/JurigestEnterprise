using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class RegistrarResultadoDiligenciaRequest
{
    public ResultadoDiligencia Resultado { get; set; }

    public string ResultadoDetalle { get; set; } =
        string.Empty;

    public string Estampe { get; set; } =
        string.Empty;

    public DateTime FechaGestion { get; set; }
}