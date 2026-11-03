using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.CambiarEstadoCausa;

// Devuelve false cuando la causa no existe.
public sealed record CambiarEstadoCausaCommand(
    Guid Id,
    EstadoCausa Estado)
    : IRequest<bool>;
