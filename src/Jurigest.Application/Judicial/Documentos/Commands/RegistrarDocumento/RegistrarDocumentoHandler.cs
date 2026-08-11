using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.RegistrarDocumento;

public sealed class RegistrarDocumentoHandler
    : IRequestHandler<RegistrarDocumentoCommand, Guid?>
{
    private readonly ICausaRepository _causaRepository;
    private readonly IDocumentoRepository _documentoRepository;

    public RegistrarDocumentoHandler(
        ICausaRepository causaRepository,
        IDocumentoRepository documentoRepository)
    {
        _causaRepository = causaRepository;
        _documentoRepository = documentoRepository;
    }

    public async Task<Guid?> Handle(
        RegistrarDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        var causaExiste = await _causaRepository.ExistsAsync(
            request.CausaId,
            cancellationToken);

        if (!causaExiste)
            return null;

        var documento = new Documento(
            Guid.NewGuid(),
            request.CausaId,
            request.Nombre,
            request.Tipo,
            request.RutaArchivo);

        await _documentoRepository.AddAsync(
            documento,
            cancellationToken);

        return documento.Id;
    }
}