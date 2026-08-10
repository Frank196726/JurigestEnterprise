using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.ProgramarDiligencia;

public sealed record ProgramarDiligenciaCommand(
    Guid Id,
    DateTime FechaProgramada)
    : IRequest<bool>;