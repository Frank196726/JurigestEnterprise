using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class SesionUsuarioRepository
    : ISesionUsuarioRepository
{
    private readonly JurigestDbContext _context;

    public SesionUsuarioRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        SesionUsuario sesion,
        CancellationToken cancellationToken)
    {
        await _context.SesionesUsuario.AddAsync(
            sesion,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SesionUsuario?>
        GetByRefreshTokenHashAsync(
            string refreshTokenHash,
            CancellationToken cancellationToken)
    {
        return await _context.SesionesUsuario
            .FirstOrDefaultAsync(
                sesion =>
                    sesion.RefreshTokenHash ==
                    refreshTokenHash,
                cancellationToken);
    }

    public async Task<SesionUsuario?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.SesionesUsuario
            .FirstOrDefaultAsync(
                sesion => sesion.Id == id,
                cancellationToken);
    }

    public async Task<List<SesionUsuario>>
        GetActiveByUsuarioIdAsync(
            Guid usuarioId,
            DateTime fechaUtc,
            CancellationToken cancellationToken)
    {
        return await _context.SesionesUsuario
            .AsNoTracking()
            .Where(sesion =>
                sesion.UsuarioId == usuarioId &&
                sesion.RevocadaUtc == null &&
                sesion.ExpiraUtc > fechaUtc)
            .OrderByDescending(
                sesion => sesion.FechaCreacionUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        SesionUsuario sesion,
        CancellationToken cancellationToken)
    {
        _context.SesionesUsuario.Update(sesion);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RotateAsync(
        SesionUsuario sesionAnterior,
        SesionUsuario sesionNueva,
        CancellationToken cancellationToken)
    {
        _context.SesionesUsuario.Update(
            sesionAnterior);

        await _context.SesionesUsuario.AddAsync(
            sesionNueva,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllActiveAsync(
        Guid usuarioId,
        DateTime fechaUtc,
        CancellationToken cancellationToken)
    {
        var sesiones = await _context.SesionesUsuario
            .Where(sesion =>
                sesion.UsuarioId == usuarioId &&
                sesion.RevocadaUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var sesion in sesiones)
        {
            sesion.Revocar(fechaUtc);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RevokeAllExceptAsync(
        Guid usuarioId,
        Guid sesionActualId,
        DateTime fechaUtc,
        CancellationToken cancellationToken)
    {
        var sesiones = await _context.SesionesUsuario
            .Where(sesion =>
                sesion.UsuarioId == usuarioId &&
                sesion.Id != sesionActualId &&
                sesion.RevocadaUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var sesion in sesiones)
        {
            sesion.Revocar(fechaUtc);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return sesiones.Count;
    }
}