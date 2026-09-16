using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class RegistrarResultadoDiligenciaRequest
{
    public Guid DiligenciaRealizadaId { get; set; }

    public ResultadoDiligencia Resultado { get; set; }

    public string ResultadoDetalle { get; set; } =
        string.Empty;

    public string Estampe { get; set; } =
        string.Empty;

    public DateTime FechaGestion { get; set; }
}
