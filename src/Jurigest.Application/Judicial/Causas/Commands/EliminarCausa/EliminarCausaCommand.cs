using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.EliminarCausa;

public sealed record EliminarCausaCommand(
    Guid Id
) : IRequest<EliminarCausaResponse>;