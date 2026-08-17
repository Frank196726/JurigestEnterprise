using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.IniciarSesion;

public sealed class IniciarSesionHandler
    : IRequestHandler<IniciarSesionCommand, IniciarSesionResponse?>
{
    private const int MaximoIntentosFallidos = 5;
    private const int DiasVigenciaRefreshToken = 30;

    private static readonly TimeSpan DuracionBloqueo =
        TimeSpan.FromMinutes(15);

    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ISesionUsuarioRepository _sesionRepository;
    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public IniciarSesionHandler(
        IUsuarioRepository repository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ISesionUsuarioRepository sesionRepository,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _sesionRepository = sesionRepository;
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
                    Limitar(request.DireccionIp, 45));

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


        var refreshToken =
            _refreshTokenService.GenerarToken();

        var refreshTokenHash =
            _refreshTokenService.CalcularHash(
                refreshToken);

        var refreshTokenExpiraUtc =
            fechaUtc.AddDays(DiasVigenciaRefreshToken);

        var sesion = new SesionUsuario(
            Guid.NewGuid(),
            usuario.Id,
            refreshTokenHash,
            usuario.VersionSeguridad,
            fechaUtc,
            refreshTokenExpiraUtc,
            Limitar(request.DireccionIp, 45),
            Limitar(request.UserAgent, 512));

        await _sesionRepository.AddAsync(
            sesion,
            cancellationToken);

        var token = _tokenService.CrearToken(
            usuario,
            sesion.Id);

        return new IniciarSesionResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            refreshToken,
            refreshTokenExpiraUtc,
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol.ToString());
    }

    private static string? Limitar(
        string? valor,
        int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var normalizado = valor.Trim();

        return normalizado.Length <= maximo
            ? normalizado
            : normalizado[..maximo];
    }
}