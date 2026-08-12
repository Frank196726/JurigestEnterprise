using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.CargarDocumento;

public sealed record CargarDocumentoCommand(
    Guid CausaId,
    string Nombre,
    TipoDocumento Tipo,
    string NombreArchivo,
    string ContentType,
    long TamanoBytes,
    Stream Contenido)
    : IRequest<Guid?>;