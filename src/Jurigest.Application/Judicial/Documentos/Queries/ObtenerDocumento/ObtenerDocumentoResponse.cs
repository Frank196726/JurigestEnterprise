using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumento;

public sealed record ObtenerDocumentoResponse(
    Guid Id,
    Guid CausaId,
    string Nombre,
    TipoDocumento Tipo,
    string RutaArchivo,
    DateTime FechaRegistro);