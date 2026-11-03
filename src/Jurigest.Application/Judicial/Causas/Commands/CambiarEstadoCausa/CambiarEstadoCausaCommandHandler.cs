using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.CambiarEstadoCausa;

public sealed class CambiarEstadoCausaCommandHandler
    : IRequestHandler<CambiarEstadoCausaCommand, bool>
{
    private readonly ICausaRepository _repository;

    public CambiarEstadoCausaCommandHandler(
        ICausaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        CambiarEstadoCausaCommand request,
        CancellationToken cancellationToken)
    {
        var causa = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (causa is null)
            return false;

        causa.CambiarEstado(request.Estado);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
