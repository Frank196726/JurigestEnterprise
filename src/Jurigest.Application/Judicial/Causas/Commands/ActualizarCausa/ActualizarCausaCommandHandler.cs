using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.ActualizarCausa;

public sealed class ActualizarCausaCommandHandler
    : IRequestHandler<ActualizarCausaCommand, ActualizarCausaResponse?>
{
    private readonly ICausaRepository _repository;

    public ActualizarCausaCommandHandler(
        ICausaRepository repository)
    {
        _repository = repository;
    }


    public async Task<ActualizarCausaResponse?> Handle(
        ActualizarCausaCommand request,
        CancellationToken cancellationToken)
    {
        var causa = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (causa is null)
            return null;


        causa.ActualizarDatos(
            request.Tribunal,
            request.Descripcion);


        await _repository.UpdateAsync(
            causa,
            cancellationToken);


        return new ActualizarCausaResponse(
            causa.Id,
            "La causa fue actualizada correctamente.");
    }
}