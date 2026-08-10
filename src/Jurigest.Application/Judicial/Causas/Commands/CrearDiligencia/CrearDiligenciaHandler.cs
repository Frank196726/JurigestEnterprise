using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CrearDiligencia;

public sealed class CrearDiligenciaHandler
    : IRequestHandler<CrearDiligenciaCommand, Guid>
{
    private readonly ICausaRepository _causaRepository;
    private readonly IDiligenciaRepository _diligenciaRepository;

    public CrearDiligenciaHandler(
        ICausaRepository causaRepository,
        IDiligenciaRepository diligenciaRepository)
    {
        _causaRepository = causaRepository;
        _diligenciaRepository = diligenciaRepository;
    }

    public async Task<Guid> Handle(
        CrearDiligenciaCommand request,
        CancellationToken cancellationToken)
    {
        var causa = await _causaRepository.GetByIdAsync(
            request.CausaId,
            cancellationToken);

        if (causa is null)
            throw new Exception("La causa no existe.");

        var diligencia = causa.AgregarDiligencia(
            request.Descripcion);

        await _diligenciaRepository.AddAsync(
            diligencia,
            cancellationToken);

        return diligencia.Id;
    }
}