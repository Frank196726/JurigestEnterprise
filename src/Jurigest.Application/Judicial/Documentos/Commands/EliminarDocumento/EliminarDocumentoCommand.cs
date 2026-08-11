using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.EliminarDocumento;

public sealed record EliminarDocumentoCommand(Guid Id)
    : IRequest<bool>;