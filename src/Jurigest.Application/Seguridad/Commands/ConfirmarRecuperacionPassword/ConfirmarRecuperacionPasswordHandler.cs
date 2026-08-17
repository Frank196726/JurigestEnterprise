using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.ConfirmarRecuperacionPassword;

public sealed class ConfirmarRecuperacionPasswordHandler
    : IRequestHandler<
        ConfirmarRecuperacionPasswordCommand,
        ConfirmarRecuperacionPasswordResultado>
{
    private readonly ITokenRecuperacionPasswordRepository
        _tokenRepository;

    private readonly ITokenRecuperacionPasswordService
        _tokenService;

    private readonly IUsuarioRepository
        _usuarioRepository;

    private readonly IPasswordHasher
        _passwordHasher;

    private readonly ISesionUsuarioRepository
        _sesionRepository;

    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public ConfirmarRecuperacionPasswordHandler(
        ITokenRecuperacionPasswordRepository tokenRepository,
        ITokenRecuperacionPasswordService tokenService,
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ISesionUsuarioRepository sesionRepository,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _tokenRepository = tokenRepository;
        _tokenService = tokenService;
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _sesionRepository = sesionRepository;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<ConfirmarRecuperacionPasswordResultado>
        Handle(
            ConfirmarRecuperacionPasswordCommand request,
            CancellationToken cancellationToken)
    {
        var tokenHash =
            _tokenService.CalcularHash(request.Token);

        var token =
            await _tokenRepository.GetByHashAsync(
                tokenHash,
                cancellationToken);

        var fechaUtc = DateTime.UtcNow;

        if (token is null ||
            !token.EstaDisponible(fechaUtc))
        {
            return ConfirmarRecuperacionPasswordResultado
                .TokenInvalido;
        }

        var usuario =
            await _usuarioRepository.GetByIdAsync(
                token.UsuarioId,
                cancellationToken);

        if (usuario is null || !usuario.Activo)
        {
            return ConfirmarRecuperacionPasswordResultado
                .TokenInvalido;
        }

        var passwordHash =
            _passwordHasher.Hash(
                request.NuevaPassword);

        usuario.CambiarPasswordHash(passwordHash);
        token.MarcarUsado(fechaUtc);

        var completada =
            await _tokenRepository.CompleteAsync(
                token,
                usuario,
                cancellationToken);

        if (!completada)
        {
            return ConfirmarRecuperacionPasswordResultado
                .TokenInvalido;
        }

        await _sesionRepository.RevokeAllActiveAsync(
            usuario.Id,
            fechaUtc,
            cancellationToken);

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            null,
            "PasswordRecuperada",
            usuario.Id,
            "La contraseña fue cambiada mediante " +
            "un token de recuperacion.",
            Limitar(request.DireccionIp, 45));

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return ConfirmarRecuperacionPasswordResultado
            .Completada;
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