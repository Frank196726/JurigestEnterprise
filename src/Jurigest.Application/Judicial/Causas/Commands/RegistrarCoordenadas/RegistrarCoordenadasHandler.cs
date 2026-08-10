using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RegistrarCoordenadas;

public sealed class RegistrarCoordenadasHandler
    : IRequestHandler<RegistrarCoordenadasCommand, bool>
{
    private readonly IDiligenciaRepository _repository;

    public RegistrarCoordenadasHandler(
        IDiligenciaRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        RegistrarCoordenadasCommand request,
        CancellationToken cancellationToken)
    {
        var diligencia = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (diligencia is null)
            return false;

        diligencia.RegistrarCoordenadas(
            request.Latitud,
            request.Longitud);

        await _repository.UpdateAsync(
            diligencia,
            cancellationToken);

        return true;
    }
}
