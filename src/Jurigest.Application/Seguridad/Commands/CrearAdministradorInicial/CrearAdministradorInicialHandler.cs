using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Domain.Seguridad.Enums;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CrearAdministradorInicial;

public sealed class CrearAdministradorInicialHandler
    : IRequestHandler<CrearAdministradorInicialCommand, Guid?>
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public CrearAdministradorInicialHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid?> Handle(
        CrearAdministradorInicialCommand request,
        CancellationToken cancellationToken)
    {
        if (await _repository.AnyAsync(cancellationToken))
            return null;

        var passwordHash = _passwordHasher.Hash(request.Password);

        var usuario = new Usuario(
            Guid.NewGuid(),
            request.Nombre,
            request.Email,
            passwordHash,
            RolUsuario.Administrador);

        await _repository.AddAsync(
            usuario,
            cancellationToken);

        return usuario.Id;
    }
}