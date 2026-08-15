using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.RestablecerPassword;

public sealed class RestablecerPasswordHandler
    : IRequestHandler<RestablecerPasswordCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public RestablecerPasswordHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<bool> Handle(
        RestablecerPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (usuario is null)
            return false;

        var nuevoHash = _passwordHasher.Hash(
            request.NuevaPassword);

        usuario.CambiarPasswordHash(nuevoHash);

        await _repository.UpdateAsync(
            usuario,
            cancellationToken);

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            request.UsuarioActorId,
            "PasswordRestablecida",
            usuario.Id,
            "Se restablecio la contraseña del usuario.",
            request.DireccionIp);

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return true;
    }
}