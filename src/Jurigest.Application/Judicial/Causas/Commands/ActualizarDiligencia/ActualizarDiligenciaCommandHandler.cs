using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.ActualizarDiligencia;

public sealed class ActualizarDiligenciaCommandHandler
    : IRequestHandler<ActualizarDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public ActualizarDiligenciaCommandHandler(IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        ActualizarDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.ActualizarDatos(
            request.Descripcion,
            request.Tipo,
            request.FechaProgramada,
            request.ReceptorJudicial,
            request.Direccion,
            request.Comuna,
            request.Observaciones);

        await _repository.UpdateAsync(diligencia, cancellationToken);
        return true;
    }
}
