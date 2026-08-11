using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Commands.RegistrarResolucion;

public sealed class RegistrarResolucionHandler
    : IRequestHandler<RegistrarResolucionCommand, Guid?>
{
    private readonly ICausaRepository _causaRepository;
    private readonly IResolucionRepository _resolucionRepository;

    public RegistrarResolucionHandler(
        ICausaRepository causaRepository,
        IResolucionRepository resolucionRepository)
    {
        _causaRepository = causaRepository;
        _resolucionRepository = resolucionRepository;
    }

    public async Task<Guid?> Handle(
        RegistrarResolucionCommand request,
        CancellationToken cancellationToken)
    {
        var causaExiste = await _causaRepository.ExistsAsync(
            request.CausaId,
            cancellationToken);

        if (!causaExiste)
            return null;

        var resolucion = new Resolucion(
            Guid.NewGuid(),
            request.CausaId,
            request.Tipo,
            request.Fecha,
            request.Descripcion);

        await _resolucionRepository.AddAsync(
            resolucion,
            cancellationToken);

        return resolucion.Id;
    }
}