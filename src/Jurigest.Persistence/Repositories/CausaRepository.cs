using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class CausaRepository : ICausaRepository
{
    public Task AddDemandadoAsync(Demandado demandado, CancellationToken cancellationToken)
    {
        _context.Demandados.Add(demandado);
        return Task.CompletedTask;
    }

    public Task AddAvalSolidarioAsync(AvalSolidario aval, CancellationToken cancellationToken)
    {
        _context.AvalesSolidarios.Add(aval);
        return Task.CompletedTask;
    }
    private readonly JurigestDbContext _context;

    public CausaRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Causa causa,
        CancellationToken cancellationToken)
    {
        await _context.Causas.AddAsync(causa, cancellationToken);
        try
        {
            await GuardarAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            _context.Entry(causa).State = EntityState.Detached;
            throw;
        }
    }

    // El índice UX_Causas_Rit_Tribunal cubre la carrera entre la validación
    // previa del handler y el guardado.
    private async Task GuardarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sql
            && sql.Message.Contains("UX_Causas_Rit_Tribunal", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                Jurigest.Domain.Judicial.IdentificacionCausa.MensajeCausaDuplicada, ex);
        }
    }

    public async Task<Causa?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Causas
            .Include(c => c.Diligencias)
            .Include(c => c.Demandados).ThenInclude(d => d.Avales)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Causas
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Causa>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Causas
            .Include(c => c.Diligencias)
            .OrderBy(c => c.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Causa causa,
        CancellationToken cancellationToken)
    {
        _context.Causas.Update(causa);

        await GuardarAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await GuardarAsync(cancellationToken);
    }
    public async Task DeleteAsync(
    Causa causa,
    CancellationToken cancellationToken)
    {
        _context.Causas.Remove(causa);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Causa?> GetByRitAsync(
    string rit,
    CancellationToken cancellationToken)
    {
        return await ConsultarPorRit(rit)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Causa>> GetAllByRitAsync(
        string rit,
        CancellationToken cancellationToken)
    {
        return await ConsultarPorRit(rit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Causa>> GetAllByNumeroRolAsync(
        string rit,
        CancellationToken cancellationToken)
    {
        var normalizado = Jurigest.Domain.Judicial.IdentificacionCausa.NormalizarRol(rit);
        var numero = Jurigest.Domain.Judicial.IdentificacionCausa.NumeroRol(normalizado);
        if (numero is null)
            return await GetAllByRitAsync(rit, cancellationToken);

        var sufijo = $"-{numero}";
        var candidatas = await _context.Causas
            .Include(c => c.Demandados)
            .Include(c => c.Diligencias)
            .Where(c => c.Rit.Trim().ToUpper() == numero || c.Rit.Trim().ToUpper().EndsWith(sufijo))
            .ToListAsync(cancellationToken);

        // El LIKE también acepta prefijos que no son una letra de tipo; se descartan aquí.
        // Primero las del ROL buscado y luego las de otras letras.
        return candidatas
            .Where(c => Jurigest.Domain.Judicial.IdentificacionCausa.NumeroRol(c.Rit) == numero)
            .OrderBy(c => Jurigest.Domain.Judicial.IdentificacionCausa.NormalizarRol(c.Rit) != normalizado)
            .ThenBy(c => c.Rit)
            .ThenBy(c => c.FechaCreacion).ThenBy(c => c.Id)
            .ToList();
    }

    private IQueryable<Causa> ConsultarPorRit(string rit)
    {
        var normalizado = Jurigest.Domain.Judicial.IdentificacionCausa.NormalizarRol(rit);
        var legado = normalizado.StartsWith("C-", StringComparison.Ordinal) ? normalizado[2..] : normalizado;
        return _context.Causas
            .Include(c => c.Demandados)
            .Include(c => c.Diligencias)
            .Where(c => c.Rit.Trim().ToUpper() == normalizado || c.Rit.Trim().ToUpper() == legado)
            .OrderBy(c => c.FechaCreacion).ThenBy(c => c.Id);
    }
}
