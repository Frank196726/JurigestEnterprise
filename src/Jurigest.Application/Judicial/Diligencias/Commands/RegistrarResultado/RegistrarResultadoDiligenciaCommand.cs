using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RegistrarResultado;

public sealed record RegistrarResultadoDiligenciaCommand(
    Guid DiligenciaId,
    Guid DiligenciaRealizadaId,
    ResultadoDiligencia Resultado,
    string ResultadoDetalle,
    string Estampe,
    DateTime FechaGestion)
    : IRequest;
