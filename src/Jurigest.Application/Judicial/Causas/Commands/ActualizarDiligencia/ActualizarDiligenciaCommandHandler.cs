using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.ActualizarDiligencia;

public sealed class ActualizarDiligenciaCommandHandler
    : IRequestHandler<ActualizarDiligenciaCommand, bool>
{
    private readonly IDiligenciaRepository _repository;
    private readonly ICausaRepository _causas;

    public ActualizarDiligenciaCommandHandler(IDiligenciaRepository repository, ICausaRepository causas)
    {
        _repository = repository;
        _causas = causas;
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

        var causa = await _causas.GetByIdAsync(diligencia.CausaId, cancellationToken)
            ?? throw new InvalidOperationException("La causa no existe.");
        causa.ValidarCorreccionDiligencia(diligencia.Id);

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
