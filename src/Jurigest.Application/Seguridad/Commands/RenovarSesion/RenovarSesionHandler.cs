using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.RenovarSesion;

public sealed class RenovarSesionHandler
    : IRequestHandler<RenovarSesionCommand, RenovarSesionResponse?>
{
    private const int DiasVigenciaRefreshToken = 30;

    private readonly ISesionUsuarioRepository _sesionRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITokenService _tokenService;
    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public RenovarSesionHandler(
        ISesionUsuarioRepository sesionRepository,
        IUsuarioRepository usuarioRepository,
        IRefreshTokenService refreshTokenService,
        ITokenService tokenService,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _sesionRepository = sesionRepository;
        _usuarioRepository = usuarioRepository;
        _refreshTokenService = refreshTokenService;
        _tokenService = tokenService;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<RenovarSesionResponse?> Handle(
        RenovarSesionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return null;

        var refreshTokenHash =
            _refreshTokenService.CalcularHash(
                request.RefreshToken);

        var sesionAnterior =
            await _sesionRepository
                .GetByRefreshTokenHashAsync(
                    refreshTokenHash,
                    cancellationToken);

        var fechaUtc = DateTime.UtcNow;

        if (sesionAnterior is null)
            return null;

        if (sesionAnterior.RevocadaUtc.HasValue)
        {
            if (sesionAnterior.ReemplazadaPorId.HasValue)
            {
                await ProcesarReutilizacionAsync(
                    sesionAnterior,
                    fechaUtc,
                    request.DireccionIp,
                    cancellationToken);
            }

            return null;
        }

        if (!sesionAnterior.EstaActiva(fechaUtc))
            return null;

        var usuario =
            await _usuarioRepository.GetByIdAsync(
                sesionAnterior.UsuarioId,
                cancellationToken);

        if (usuario is null ||
            !usuario.Activo ||
            usuario.VersionSeguridad !=
                sesionAnterior.VersionSeguridad)
        {
            sesionAnterior.Revocar(fechaUtc);

            await _sesionRepository.UpdateAsync(
                sesionAnterior,
                cancellationToken);

            return null;
        }

        var nuevoRefreshToken =
            _refreshTokenService.GenerarToken();

        var nuevoRefreshTokenHash =
            _refreshTokenService.CalcularHash(
                nuevoRefreshToken);

        var nuevoRefreshTokenExpiraUtc =
            fechaUtc.AddDays(DiasVigenciaRefreshToken);

        var sesionNueva = new SesionUsuario(
            Guid.NewGuid(),
            usuario.Id,
            nuevoRefreshTokenHash,
            usuario.VersionSeguridad,
            fechaUtc,
            nuevoRefreshTokenExpiraUtc,
            Limitar(request.DireccionIp, 45),
            Limitar(request.UserAgent, 512));

        sesionAnterior.Revocar(
            fechaUtc,
            sesionNueva.Id);

        await _sesionRepository.RotateAsync(
            sesionAnterior,
            sesionNueva,
            cancellationToken);

        var accessToken =
            _tokenService.CrearToken(
            usuario,
            sesionNueva.Id);

        return new RenovarSesionResponse(
            accessToken.AccessToken,
            accessToken.ExpiresAtUtc,
            nuevoRefreshToken,
            nuevoRefreshTokenExpiraUtc);
    }

    private async Task ProcesarReutilizacionAsync(
        SesionUsuario sesionReutilizada,
        DateTime fechaUtc,
        string? direccionIp,
        CancellationToken cancellationToken)
    {
        var usuario =
            await _usuarioRepository.GetByIdAsync(
                sesionReutilizada.UsuarioId,
                cancellationToken);

        if (usuario is null)
            return;

        usuario.InvalidarSesiones();

        await _usuarioRepository.UpdateAsync(
            usuario,
            cancellationToken);

        await _sesionRepository.RevokeAllActiveAsync(
            usuario.Id,
            fechaUtc,
            cancellationToken);

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            null,
            "RefreshTokenReutilizado",
            usuario.Id,
            "Se detecto la reutilizacion de un refresh token. " +
            "Todas las sesiones del usuario fueron invalidadas.",
            Limitar(direccionIp, 45));

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);
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