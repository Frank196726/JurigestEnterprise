using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.IniciarSesion;

public sealed class IniciarSesionHandler
    : IRequestHandler<IniciarSesionCommand, IniciarSesionResponse?>
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public IniciarSesionHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<IniciarSesionResponse?> Handle(
        IniciarSesionCommand request,
        CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (usuario is null ||
            !usuario.Activo ||
            !_passwordHasher.Verify(
                request.Password,
                usuario.PasswordHash))
        {
            return null;
        }

        var token = _tokenService.CrearToken(usuario);

        return new IniciarSesionResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol.ToString());
    }
}