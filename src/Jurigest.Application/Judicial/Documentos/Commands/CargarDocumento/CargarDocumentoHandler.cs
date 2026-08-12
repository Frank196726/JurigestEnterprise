using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Storage;
using Jurigest.Application.Judicial.Documentos.Validation;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.CargarDocumento;

public sealed class CargarDocumentoHandler
    : IRequestHandler<CargarDocumentoCommand, Guid?>
{
    private const long MaximoBytes = 10 * 1024 * 1024;

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
        if (request.TamanoBytes <= 0 ||
            request.TamanoBytes > MaximoBytes)
        {
            throw new ArgumentException(
                "El archivo debe tener un tamaño entre 1 byte y 10 MB.");
        }

        var causaExiste = await _causaRepository.ExistsAsync(
            request.CausaId,
            cancellationToken);

        if (!causaExiste)
            return null;

        var contentType =
            await ArchivoDocumentoValidator.ValidarAsync(
                request.Contenido,
                request.NombreArchivo,
                cancellationToken);

        request.Contenido.Position = 0;

        var extension = Path.GetExtension(request.NombreArchivo);

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
                contentType,
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