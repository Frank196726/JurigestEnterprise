namespace Jurigest.Web.Models;

public sealed record ReciboResumen(
    Guid Id,
    Guid CausaId,
    Guid DiligenciaId,
    Guid DiligenciaRealizadaId,
    string DiligenciaRealizada,
    decimal Monto,
    int Estado,
    DateTime FechaEmision,
    DateTime? FechaPago)
{
    public long Numero { get; init; }
    public string? Abogado { get; init; }
    public string? Demandante { get; init; }
    public string? Receptor { get; init; }
    public string? Rol { get; init; }
    public string? Tribunal { get; init; }
    public string? Caratulado { get; init; }
    public string? DiligenciaEncargada { get; init; }
    public string? NumeroOperacion { get; init; }
    public decimal? Cuantia { get; init; }
    public decimal ValorGestion { get; init; }
    public string? Observacion { get; init; }
    public string? DetalleAdicionales { get; init; }
    public decimal TotalAdicionales { get; init; }
}
