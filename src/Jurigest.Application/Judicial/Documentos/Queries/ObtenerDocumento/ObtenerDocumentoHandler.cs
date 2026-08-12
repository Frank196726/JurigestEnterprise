using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Queries.ObtenerDocumento;

public sealed class ObtenerDocumentoHandler
    : IRequestHandler<ObtenerDocumentoQuery, ObtenerDocumentoResponse?>
{
    private readonly IDocumentoRepository _repository;

    public ObtenerDocumentoHandler(
        IDocumentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ObtenerDocumentoResponse?> Handle(
        ObtenerDocumentoQuery request,
        CancellationToken cancellationToken)
    {
        var documento = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (documento is null)
            return null;

        return new ObtenerDocumentoResponse(
            documento.Id,
            documento.CausaId,
            documento.Nombre,
            documento.Tipo,
            documento.RutaArchivo,
            documento.ContentType,
            documento.TamanoBytes,
            documento.FechaRegistro);
    }
}