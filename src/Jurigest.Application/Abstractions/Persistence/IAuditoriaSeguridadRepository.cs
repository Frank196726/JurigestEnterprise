using Jurigest.Domain.Seguridad.Entities;

namespace Jurigest.Application.Abstractions.Persistence;

public interface IAuditoriaSeguridadRepository
{
    Task AddAsync(
        AuditoriaSeguridad auditoria,
        CancellationToken cancellationToken);

    Task<List<AuditoriaSeguridad>> GetLatestAsync(
        int cantidad,
        CancellationToken cancellationToken);
}