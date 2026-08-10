using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.SuspenderDiligencia;

public sealed class SuspenderDiligenciaHandler
    : IRequestHandler<SuspenderDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public SuspenderDiligenciaHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        SuspenderDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.Suspender();

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}
