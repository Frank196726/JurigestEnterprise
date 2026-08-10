using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.AsignarUbicacion;

public sealed record AsignarUbicacionCommand(
    Guid Id,
    string Direccion,
    string Comuna)
    : IRequest<bool>;
