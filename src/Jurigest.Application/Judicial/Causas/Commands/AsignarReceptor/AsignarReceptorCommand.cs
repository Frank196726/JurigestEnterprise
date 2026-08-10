using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.AsignarReceptor;

public sealed record AsignarReceptorCommand(
    Guid Id,
    string ReceptorJudicial)
    : IRequest<bool>;