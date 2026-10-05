using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RegistrarResultado;

public sealed record RegistrarResultadoDiligenciaCommand(
    Guid DiligenciaId,
    Guid DiligenciaRealizadaId,
    ResultadoDiligencia Resultado,
    string? ResultadoDetalle,
    string Estampe,
    DateTime FechaGestion,
    decimal? Monto = null)
    : IRequest
{
    public string? Abogado { get; init; }
    public string? NumeroOperacion { get; init; }
    public decimal? Cuantia { get; init; }
    public string? ObservacionRecibo { get; init; }
    public string? DetalleAdicionales { get; init; }
    public decimal TotalAdicionales { get; init; }
}
