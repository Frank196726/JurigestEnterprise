using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.AsignarUbicacion;

public sealed class AsignarUbicacionHandler
    : IRequestHandler<AsignarUbicacionCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public AsignarUbicacionHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        AsignarUbicacionCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.AsignarUbicacion(
            request.Direccion,
            request.Comuna);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}
