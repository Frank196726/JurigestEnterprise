using Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumento;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumentosPorCausa;

public sealed record ObtenerDocumentosPorCausaQuery(Guid CausaId)
    : IRequest<List<ObtenerDocumentoResponse>>;