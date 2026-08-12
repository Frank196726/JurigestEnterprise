using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Storage;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.CargarDocumento;

public sealed class CargarDocumentoHandler
    : IRequestHandler<CargarDocumentoCommand, Guid?>
{
    private readonly ICausaRepository _causaRepository;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IArchivoStorage _archivoStorage;

    public CargarDocumentoHandler(
        ICausaRepository causaRepository,
        IDocumentoRepository documentoRepository,
        IArchivoStorage archivoStorage)
    {
        _causaRepository = causaRepository;
        _documentoRepository = documentoRepository;
        _archivoStorage = archivoStorage;
    }

    public async Task<Guid?> Handle(
        CargarDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        var causaExiste = await _causaRepository.ExistsAsync(
            request.CausaId,
            cancellationToken);

        if (!causaExiste)
            return null;

        var extension = Path.GetExtension(request.NombreArchivo);

        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException(
                "El archivo debe tener una extension valida.");

        var rutaArchivo = await _archivoStorage.GuardarAsync(
            request.Contenido,
            extension,
            cancellationToken);

        try
        {
            var documento = new Documento(
                Guid.NewGuid(),
                request.CausaId,
                request.Nombre,
                request.Tipo,
                rutaArchivo,
                request.ContentType,
                request.TamanoBytes);

            await _documentoRepository.AddAsync(
                documento,
                cancellationToken);

            return documento.Id;
        }
        catch
        {
            await _archivoStorage.EliminarAsync(
                rutaArchivo,
                CancellationToken.None);

            throw;
        }
    }
}