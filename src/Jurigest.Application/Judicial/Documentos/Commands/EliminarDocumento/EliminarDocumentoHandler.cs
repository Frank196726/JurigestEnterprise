using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Documentos.Commands.EliminarDocumento;

public sealed class EliminarDocumentoHandler
    : IRequestHandler<EliminarDocumentoCommand, bool>
{
    private readonly IDocumentoRepository _repository;

    public EliminarDocumentoHandler(IDocumentoRepository repository)
    {
        _repository = repository;
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

        return true;
    }
}