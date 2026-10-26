using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Seguridad.Entities;
using Jurigest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly JurigestDbContext _context;

    public UsuarioRepository(JurigestDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Usuario usuario,
        CancellationToken cancellationToken)
    {
        await _context.Usuarios.AddAsync(
            usuario,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(
                usuario => usuario.Id == id,
                cancellationToken);
    }

    public async Task<List<Usuario>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var emailNormalizado =
            email.Trim().ToLowerInvariant();

        return await _context.Usuarios
            .FirstOrDefaultAsync(
                usuario =>
                    usuario.Email == emailNormalizado,
                cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var emailNormalizado =
            email.Trim().ToLowerInvariant();

        return await _context.Usuarios.AnyAsync(
            usuario =>
                usuario.Email == emailNormalizado,
            cancellationToken);
    }

    public async Task<bool> ExistsByRutAsync(
        string rut,
        CancellationToken cancellationToken)
    {
        var normalizado = Jurigest.Domain.Judicial.RutChileno.Normalizar(rut);
        return await _context.Usuarios.AnyAsync(
            usuario => usuario.Rut == normalizado,
            cancellationToken);
    }

    public Task<bool> AnyAsync(
        CancellationToken cancellationToken)
    {
        return _context.Usuarios.AnyAsync(
            cancellationToken);
    }

    public async Task AddAsync(
        Usuario usuario,
        IReadOnlyCollection<Guid> receptorIds,
        CancellationToken cancellationToken)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        foreach (var receptorId in receptorIds.Distinct())
            await _context.UsuariosReceptores.AddAsync(
                new UsuarioReceptor(usuario.Id, receptorId),
                cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Usuario usuario,
        IReadOnlyCollection<Guid> receptorIds,
        CancellationToken cancellationToken)
    {
        var solicitados = receptorIds.Distinct().ToHashSet();
        var actuales = await _context.UsuariosReceptores
            .Where(x => x.UsuarioId == usuario.Id)
            .ToListAsync(cancellationToken);

        _context.UsuariosReceptores.RemoveRange(
            actuales.Where(x => !solicitados.Contains(x.ReceptorJudicialId)));

        foreach (var receptorId in solicitados.Except(actuales.Select(x => x.ReceptorJudicialId)))
            await _context.UsuariosReceptores.AddAsync(
                new UsuarioReceptor(usuario.Id, receptorId),
                cancellationToken);

        await UpdateAsync(usuario, cancellationToken);
    }

    public async Task<List<UsuarioReceptorAsignado>> GetReceptoresAsignadosAsync(
        IReadOnlyCollection<Guid>? usuarioIds,
        CancellationToken cancellationToken)
    {
        var consulta = _context.UsuariosReceptores.AsNoTracking();
        if (usuarioIds is not null)
            consulta = consulta.Where(x => usuarioIds.Contains(x.UsuarioId));

        // Se ordena antes de construir el record: SQL Server no puede traducir un
        // OrderBy sobre las propiedades de un objeto creado con constructor.
        return await consulta
            .Join(
                _context.ReceptoresJudiciales,
                asignacion => asignacion.ReceptorJudicialId,
                receptor => receptor.Id,
                (asignacion, receptor) => new
                {
                    asignacion.UsuarioId,
                    ReceptorId = receptor.Id,
                    receptor.Nombre
                })
            .OrderBy(x => x.Nombre)
            .Select(x => new UsuarioReceptorAsignado(x.UsuarioId, x.ReceptorId, x.Nombre))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
    Usuario usuario,
    CancellationToken cancellationToken)
    {
        var valoresGuardados = await _context.Entry(usuario).GetDatabaseValuesAsync(cancellationToken);
        var nombreAnterior = valoresGuardados?.GetValue<string>(nameof(Usuario.Nombre));
        if (!string.IsNullOrWhiteSpace(nombreAnterior) && nombreAnterior != usuario.Nombre)
        {
            // La asignación existente usa el nombre. Mantener la misma identidad al renombrar,
            // incluso en diligencias finalizadas, sin cambiar sus resultados o estampes.
            var diligencias = await _context.Diligencias
                .Where(d => d.ReceptorJudicial != null && d.ReceptorJudicial.Trim() == nombreAnterior)
                .ToListAsync(cancellationToken);
            foreach (var diligencia in diligencias)
                _context.Entry(diligencia).Property(d => d.ReceptorJudicial).CurrentValue = usuario.Nombre;
            var receptores = await _context.ReceptoresJudiciales.Where(r => r.Nombre == nombreAnterior).ToListAsync(cancellationToken);
            if (!await _context.ReceptoresJudiciales.AnyAsync(r => r.Nombre == usuario.Nombre, cancellationToken))
                foreach (var receptor in receptores)
                    _context.Entry(receptor).Property(r => r.Nombre).CurrentValue = usuario.Nombre;
        }
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
