using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CompletarDiligencia;

public sealed class CompletarDiligenciaHandler
    : IRequestHandler<CompletarDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public CompletarDiligenciaHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        CompletarDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.Completar();

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}