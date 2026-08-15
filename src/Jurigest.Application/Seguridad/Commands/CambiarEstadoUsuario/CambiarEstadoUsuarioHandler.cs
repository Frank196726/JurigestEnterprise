using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CambiarEstadoUsuario;

public sealed class CambiarEstadoUsuarioHandler
    : IRequestHandler<
        CambiarEstadoUsuarioCommand,
        CambiarEstadoUsuarioResultado>
{
    private readonly IUsuarioRepository _repository;
    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public CambiarEstadoUsuarioHandler(
        IUsuarioRepository repository,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _repository = repository;
        _auditoriaRepository = auditoriaRepository;
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

        var accion = request.Activo
            ? "UsuarioActivado"
            : "UsuarioDesactivado";

        var detalle = request.Activo
            ? "Se activo la cuenta del usuario."
            : "Se desactivo la cuenta del usuario.";

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            request.AdministradorId,
            accion,
            usuario.Id,
            detalle,
            request.DireccionIp);

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return CambiarEstadoUsuarioResultado.Actualizado;
    }
}