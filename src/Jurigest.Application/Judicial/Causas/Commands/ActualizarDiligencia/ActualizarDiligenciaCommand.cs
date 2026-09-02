using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.ActualizarDiligencia;

public sealed record ActualizarDiligenciaCommand(
    Guid Id,
    string Descripcion,
    TipoDiligencia Tipo,
    DateTime? FechaProgramada,
    string? ReceptorJudicial,
    string? Direccion,
    string? Comuna,
    string? Observaciones) : IRequest<bool>;
