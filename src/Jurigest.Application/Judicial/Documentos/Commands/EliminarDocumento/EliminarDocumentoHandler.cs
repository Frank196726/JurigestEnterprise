using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Storage;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.EliminarDocumento;

public sealed class EliminarDocumentoHandler
    : IRequestHandler<EliminarDocumentoCommand, bool>
{
    private readonly IDocumentoRepository _repository;
    private readonly IArchivoStorage _archivoStorage;

    public EliminarDocumentoHandler(
        IDocumentoRepository repository,
        IArchivoStorage archivoStorage)
    {
        _repository = repository;
        _archivoStorage = archivoStorage;
    }

    public async Task<bool> Handle(
        EliminarDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        var documento = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (documento is null)
            return false;

        await _repository.DeleteAsync(
            documento,
            cancellationToken);

        var esArchivoAdministrado =
            string.Equals(
                Path.GetFileName(documento.RutaArchivo),
                documento.RutaArchivo,
                StringComparison.Ordinal);

        if (esArchivoAdministrado)
        {
            await _archivoStorage.EliminarAsync(
                documento.RutaArchivo,
                cancellationToken);
        }

        return true;
    }
}