using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.CrearCausa;

/// <summary>
/// Handler encargado de procesar el comando de creación de una causa.
/// </summary>
public sealed class CrearCausaCommandHandler
    : IRequestHandler<CrearCausaCommand, CrearCausaResponse>
{
    private readonly ICausaRepository _causaRepository;

    public CrearCausaCommandHandler(ICausaRepository causaRepository)
    {
        _causaRepository = causaRepository;
    }

    public async Task<CrearCausaResponse> Handle(
    CrearCausaCommand request,
    CancellationToken cancellationToken)
{
    var causa = new Causa(
        request.Rit,
        request.Tribunal,
        request.Descripcion);

    await _causaRepository.AddAsync(
        causa,
        cancellationToken);

    return new CrearCausaResponse(
        causa.Id,
        true,
        "La causa fue creada correctamente.");
    }

}