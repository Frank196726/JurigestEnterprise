using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Commands.EliminarResolucion;

public sealed record EliminarResolucionCommand(Guid Id)
    : IRequest<bool>;