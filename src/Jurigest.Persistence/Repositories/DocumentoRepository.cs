using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class DocumentoRepository : IDocumentoRepository
{
    private readonly JurigestDbContext _context;

    public DocumentoRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Documento documento,
        CancellationToken cancellationToken)
    {
        await _context.Documentos.AddAsync(
            documento,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Documento?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Documentos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                documento => documento.Id == id,
                cancellationToken);
    }

    public async Task<List<Documento>> GetByCausaIdAsync(
        Guid causaId,
        CancellationToken cancellationToken)
    {
        return await _context.Documentos
            .AsNoTracking()
            .Where(documento => documento.CausaId == causaId)
            .OrderByDescending(documento => documento.FechaRegistro)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Documento documento,
        CancellationToken cancellationToken)
    {
        _context.Documentos.Remove(documento);
        await _context.SaveChangesAsync(cancellationToken);
    }
}