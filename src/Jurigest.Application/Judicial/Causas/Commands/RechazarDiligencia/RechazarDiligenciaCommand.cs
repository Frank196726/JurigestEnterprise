using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RechazarDiligencia;

public sealed record RechazarDiligenciaCommand(Guid Id)
    : IRequest<bool>;
