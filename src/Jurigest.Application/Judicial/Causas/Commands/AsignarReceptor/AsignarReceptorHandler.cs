using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.AsignarReceptor;

public sealed class AsignarReceptorHandler
    : IRequestHandler<AsignarReceptorCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public AsignarReceptorHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        AsignarReceptorCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.AsignarReceptor(
            request.ReceptorJudicial);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}