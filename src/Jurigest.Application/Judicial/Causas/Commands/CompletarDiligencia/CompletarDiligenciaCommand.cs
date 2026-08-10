using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CompletarDiligencia;

public sealed record CompletarDiligenciaCommand(Guid Id)
    : IRequest<bool>;