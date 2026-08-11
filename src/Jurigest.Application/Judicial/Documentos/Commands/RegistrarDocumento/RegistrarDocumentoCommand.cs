using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.RegistrarDocumento;

public sealed record RegistrarDocumentoCommand(
    Guid CausaId,
    string Nombre,
    TipoDocumento Tipo,
    string RutaArchivo)
    : IRequest<Guid?>;