using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CambiarTipoDiligencia;

public sealed record CambiarTipoDiligenciaCommand(
    Guid Id,
    TipoDiligencia Tipo) : IRequest<bool>;
