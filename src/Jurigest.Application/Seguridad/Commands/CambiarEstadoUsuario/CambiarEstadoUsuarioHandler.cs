using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CambiarEstadoUsuario;

public sealed class CambiarEstadoUsuarioHandler
    : IRequestHandler<
        CambiarEstadoUsuarioCommand,
        CambiarEstadoUsuarioResultado>
{
    private readonly IUsuarioRepository _repository;

    public CambiarEstadoUsuarioHandler(
        IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<CambiarEstadoUsuarioResultado> Handle(
        CambiarEstadoUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByIdAsync(
            request.UsuarioId,
            cancellationToken);

        if (usuario is null)
        {
            return CambiarEstadoUsuarioResultado.NoEncontrado;
        }

        if (!request.Activo &&
            request.UsuarioId == request.AdministradorId)
        {
            return CambiarEstadoUsuarioResultado
                .AutodesactivacionNoPermitida;
        }

        if (request.Activo)
            usuario.Activar();
        else
            usuario.Desactivar();

        await _repository.UpdateAsync(
            usuario,
            cancellationToken);

        return CambiarEstadoUsuarioResultado.Actualizado;
    }
}