using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CerrarSesionPorId;

public sealed class CerrarSesionPorIdHandler
    : IRequestHandler<CerrarSesionPorIdCommand, bool>
{
    private readonly ISesionUsuarioRepository
        _sesionRepository;

    private readonly IAuditoriaSeguridadRepository
        _auditoriaRepository;

    public CerrarSesionPorIdHandler(
        ISesionUsuarioRepository sesionRepository,
        IAuditoriaSeguridadRepository auditoriaRepository)
    {
        _sesionRepository = sesionRepository;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<bool> Handle(
        CerrarSesionPorIdCommand request,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.GetByIdAsync(
            request.SesionId,
            cancellationToken);

        if (sesion is null ||
            sesion.UsuarioId != request.UsuarioId)
        {
            return false;
        }

        if (sesion.RevocadaUtc.HasValue)
            return true;

        var fechaUtc = DateTime.UtcNow;

        sesion.Revocar(fechaUtc);

        await _sesionRepository.UpdateAsync(
            sesion,
            cancellationToken);

        var auditoria = new AuditoriaSeguridad(
            Guid.NewGuid(),
            request.UsuarioId,
            "SesionRevocada",
            request.UsuarioId,
            $"Se revoco la sesion {sesion.Id}.",
            Limitar(request.DireccionIp, 45));

        await _auditoriaRepository.AddAsync(
            auditoria,
            cancellationToken);

        return true;
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