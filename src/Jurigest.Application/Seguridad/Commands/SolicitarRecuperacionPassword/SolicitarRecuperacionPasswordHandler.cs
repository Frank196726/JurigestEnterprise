using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Abstractions.Security;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.SolicitarRecuperacionPassword;

public sealed class SolicitarRecuperacionPasswordHandler
    : IRequestHandler<
        SolicitarRecuperacionPasswordCommand,
        SolicitarRecuperacionPasswordResponse?>
{
    private static readonly TimeSpan Vigencia =
        TimeSpan.FromMinutes(30);

    private readonly IUsuarioRepository _usuarioRepository;

    private readonly ITokenRecuperacionPasswordRepository
        _tokenRepository;

    private readonly ITokenRecuperacionPasswordService
        _tokenService;

    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public SolicitarRecuperacionPasswordHandler(
        IUsuarioRepository usuarioRepository,
        ITokenRecuperacionPasswordRepository tokenRepository,
        ITokenRecuperacionPasswordService tokenService,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _tokenRepository = tokenRepository;
        _tokenService = tokenService;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<SolicitarRecuperacionPasswordResponse?>
        Handle(
            SolicitarRecuperacionPasswordCommand request,
            CancellationToken cancellationToken)
    {
        var tokenOriginal =
            _tokenService.GenerarToken();

        var tokenHash =
            _tokenService.CalcularHash(tokenOriginal);

        var usuario =
            await _usuarioRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (usuario is null || !usuario.Activo)
            return null;

        var fechaUtc = DateTime.UtcNow;
        var expiraUtc = fechaUtc.Add(Vigencia);

        var token = new TokenRecuperacionPassword(
            Guid.NewGuid(),
            usuario.Id,
            tokenHash,
            fechaUtc,
            expiraUtc);

        await _tokenRepository.ReplaceActiveAsync(
            token,
            fechaUtc,
            cancellationToken);

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            null,
            "RecuperacionPasswordSolicitada",
            usuario.Id,
            "Se genero un token de recuperacion " +
            "con vigencia de 30 minutos.",
            Limitar(request.DireccionIp, 45));

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return new SolicitarRecuperacionPasswordResponse(
            tokenOriginal,
            expiraUtc);
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