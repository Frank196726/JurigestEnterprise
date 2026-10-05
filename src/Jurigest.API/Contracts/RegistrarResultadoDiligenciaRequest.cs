using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.API.Contracts;

public sealed class RegistrarResultadoDiligenciaRequest
{
    public Guid DiligenciaRealizadaId { get; set; }

    public ResultadoDiligencia Resultado { get; set; }

    public string? ResultadoDetalle { get; set; }

    public string Estampe { get; set; } =
        string.Empty;

    public DateTime FechaGestion { get; set; }
    public decimal? Monto { get; set; }
    public string? Abogado { get; set; }
    public string? NumeroOperacion { get; set; }
    public decimal? Cuantia { get; set; }
    public string? ObservacionRecibo { get; set; }
    public string? DetalleAdicionales { get; set; }
    public decimal TotalAdicionales { get; set; }

}
