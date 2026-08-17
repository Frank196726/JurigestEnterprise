using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CerrarOtrasSesiones;

public sealed class CerrarOtrasSesionesHandler
    : IRequestHandler<CerrarOtrasSesionesCommand, int>
{
    private readonly ISesionUsuarioRepository
        _sesionRepository;

    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public CerrarOtrasSesionesHandler(
        ISesionUsuarioRepository sesionRepository,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _sesionRepository = sesionRepository;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<int> Handle(
        CerrarOtrasSesionesCommand request,
        CancellationToken cancellationToken)
    {
        var fechaUtc = DateTime.UtcNow;

        var cantidad =
            await _sesionRepository.RevokeAllExceptAsync(
                request.UsuarioId,
                request.SesionActualId,
                fechaUtc,
                cancellationToken);

        if (cantidad == 0)
            return 0;

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            request.UsuarioId,
            "OtrasSesionesRevocadas",
            request.UsuarioId,
            $"Se revocaron {cantidad} sesiones.",
            Limitar(request.DireccionIp, 45));

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return cantidad;
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