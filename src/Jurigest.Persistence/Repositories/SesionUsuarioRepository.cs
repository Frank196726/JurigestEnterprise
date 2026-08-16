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
}