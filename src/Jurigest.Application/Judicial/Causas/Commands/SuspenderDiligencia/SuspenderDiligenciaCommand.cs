using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.SuspenderDiligencia;

public sealed record SuspenderDiligenciaCommand(Guid Id)
    : IRequest<bool>;
