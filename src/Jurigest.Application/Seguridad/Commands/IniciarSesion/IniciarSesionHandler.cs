using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.IniciarSesion;

public sealed class IniciarSesionHandler
    : IRequestHandler<IniciarSesionCommand, IniciarSesionResponse?>
{
    private const int MaximoIntentosFallidos = 5;

    private static readonly TimeSpan DuracionBloqueo =
        TimeSpan.FromMinutes(15);

    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public IniciarSesionHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<IniciarSesionResponse?> Handle(
        IniciarSesionCommand request,
        CancellationToken cancellationToken)
    {
        var usuario = await _repository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (usuario is null || !usuario.Activo)
            return null;

        var fechaUtc = DateTime.UtcNow;

        if (usuario.EstaBloqueado(fechaUtc))
            return null;

        var passwordValida = _passwordHasher.Verify(
            request.Password,
            usuario.PasswordHash);

        if (!passwordValida)
        {
            var fueBloqueado =
                usuario.RegistrarIntentoFallido(
                    fechaUtc,
                    MaximoIntentosFallidos,
                    DuracionBloqueo);

            await _repository.UpdateAsync(
                usuario,
                cancellationToken);

            if (fueBloqueado)
            {
                var auditoria = new AuditoriaSeguridad(
                    Guid.NewGuid(),
                    null,
                    "UsuarioBloqueadoPorIntentosFallidos",
                    usuario.Id,
                    "La cuenta fue bloqueada temporalmente " +
                    "durante 15 minutos.",
                    request.DireccionIp);

                await _auditoriaRepository.AddAsync(
                    auditoria,
                    cancellationToken);
            }

            return null;
        }

        if (usuario.IntentosFallidos > 0 ||
            usuario.BloqueadoHastaUtc.HasValue)
        {
            usuario.RestablecerIntentosFallidos();

            await _repository.UpdateAsync(
                usuario,
                cancellationToken);
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