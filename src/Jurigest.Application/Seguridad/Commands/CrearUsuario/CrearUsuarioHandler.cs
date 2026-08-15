using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CrearUsuario;

public sealed class CrearUsuarioHandler
    : IRequestHandler<CrearUsuarioCommand, CrearUsuarioResult>
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public CrearUsuarioHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<CrearUsuarioResult> Handle(
        CrearUsuarioCommand request,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByEmailAsync(
                request.Email,
                cancellationToken))
        {
            return new CrearUsuarioResult(
                false,
                true,
                null);
        }

        var passwordHash =
            _passwordHasher.Hash(request.Password);

        var usuario = new Usuario(
            Guid.NewGuid(),
            request.Nombre,
            request.Email,
            passwordHash,
            request.Rol);

        await _repository.AddAsync(
            usuario,
            cancellationToken);

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            request.UsuarioActorId,
            "UsuarioCreado",
            usuario.Id,
            $"Rol asignado: {usuario.Rol}.",
            request.DireccionIp);

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return new CrearUsuarioResult(
            true,
            false,
            usuario.Id);
    }
}