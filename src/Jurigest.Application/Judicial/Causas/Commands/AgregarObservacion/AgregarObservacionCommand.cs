using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.AgregarObservacion;

public sealed record AgregarObservacionCommand(
    Guid Id,
    string Observacion)
    : IRequest<bool>;
