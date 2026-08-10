using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.AgregarObservacion;

public sealed class AgregarObservacionHandler
    : IRequestHandler<AgregarObservacionCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public AgregarObservacionHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        AgregarObservacionCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.AgregarObservacion(request.Observacion);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}
