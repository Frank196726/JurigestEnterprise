using MediatR;

namespace Jurigest.Application.Judicial.Recibos.Commands.MarcarPagado;

public sealed record MarcarReciboPagadoCommand(
    Guid ReciboId,
    DateTime FechaPago)
    : IRequest<bool>;
