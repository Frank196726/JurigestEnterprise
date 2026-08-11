using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumento;

public sealed record ObtenerDocumentoQuery(Guid Id)
    : IRequest<ObtenerDocumentoResponse?>;