using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumento;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumentosPorCausa;

public sealed class ObtenerDocumentosPorCausaHandler
    : IRequestHandler<
        ObtenerDocumentosPorCausaQuery,
        List<ObtenerDocumentoResponse>>
{
    private readonly IDocumentoRepository _repository;

    public ObtenerDocumentosPorCausaHandler(
        IDocumentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ObtenerDocumentoResponse>> Handle(
        ObtenerDocumentosPorCausaQuery request,
        CancellationToken cancellationToken)
    {
        var documentos = await _repository.GetByCausaIdAsync(
            request.CausaId,
            cancellationToken);

        return documentos
            .Select(documento => new ObtenerDocumentoResponse(
                documento.Id,
                documento.CausaId,
                documento.Nombre,
                documento.Tipo,
                documento.RutaArchivo,
                documento.FechaRegistro))
            .ToList();
    }
}