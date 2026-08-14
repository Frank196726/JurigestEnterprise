using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.RestablecerPassword;

public sealed class RestablecerPasswordHandler
    : IRequestHandler<RestablecerPasswordCommand, bool>
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public RestablecerPasswordHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
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

        return true;
    }
}