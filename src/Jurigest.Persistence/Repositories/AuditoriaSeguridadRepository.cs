using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class AuditoriaSeguridadRepository
    : IAuditoriaSeguridadRepository
{
    private readonly JurigestDbContext _context;

    public AuditoriaSeguridadRepository(
        JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        AuditoriaSeguridad auditoria,
        CancellationToken cancellationToken)
    {
        await _context.AuditoriasSeguridad.AddAsync(
            auditoria,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AuditoriaSeguridad>> GetLatestAsync(
        int cantidad,
        CancellationToken cancellationToken)
    {
        cantidad = Math.Clamp(cantidad, 1, 500);

        return await _context.AuditoriasSeguridad
            .AsNoTracking()
            .OrderByDescending(auditoria => auditoria.FechaUtc)
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }
}