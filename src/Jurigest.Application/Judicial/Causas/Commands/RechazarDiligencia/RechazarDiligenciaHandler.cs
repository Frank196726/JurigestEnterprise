using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RechazarDiligencia;

public sealed class RechazarDiligenciaHandler
    : IRequestHandler<RechazarDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public RechazarDiligenciaHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        RechazarDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.Rechazar();

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}
