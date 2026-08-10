using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.EliminarCausa;

public sealed class EliminarCausaCommandHandler
    : IRequestHandler<EliminarCausaCommand, EliminarCausaResponse>
{
    private readonly ICausaRepository _repository;

    public EliminarCausaCommandHandler(
        ICausaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EliminarCausaResponse> Handle(
        EliminarCausaCommand request,
        CancellationToken cancellationToken)
    {
        var causa = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (causa is null)
        {
            throw new Exception("La causa no existe.");
        }

        await _repository.DeleteAsync(
            causa,
            cancellationToken);

        return new EliminarCausaResponse(
            causa.Id,
            "La causa fue eliminada correctamente.");
    }
}