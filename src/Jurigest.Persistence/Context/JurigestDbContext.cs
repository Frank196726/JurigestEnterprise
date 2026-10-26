using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Persistence.Context;

public class JurigestDbContext : DbContext
{
    public JurigestDbContext(DbContextOptions<JurigestDbContext> options)
        : base(options)
    {
    }

    public DbSet<MateriaCatalogo> Materias => Set<MateriaCatalogo>();

    public DbSet<VehiculoOpcion> VehiculoOpciones => Set<VehiculoOpcion>();

    public DbSet<VehiculoEncargo> VehiculosEncargo => Set<VehiculoEncargo>();

    public DbSet<Causa> Causas => Set<Causa>();
    public DbSet<Demandado> Demandados => Set<Demandado>();
    public DbSet<AvalSolidario> AvalesSolidarios => Set<AvalSolidario>();

    public DbSet<Diligencia> Diligencias => Set<Diligencia>();

    public DbSet<Recibo> Recibos => Set<Recibo>();

    public DbSet<ModeloEstampe> ModelosEstampe => Set<ModeloEstampe>();

    public DbSet<TipoDiligenciaCatalogo> TiposDiligencia =>
    Set<TipoDiligenciaCatalogo>();

    public DbSet<TipoCausaCatalogo> TiposCausa =>
    Set<TipoCausaCatalogo>();

    public DbSet<ReceptorJudicialCatalogo> ReceptoresJudiciales =>
    Set<ReceptorJudicialCatalogo>();

    public DbSet<ComunaCatalogo> Comunas =>
    Set<ComunaCatalogo>();

    public DbSet<TribunalCatalogo> Tribunales =>
    Set<TribunalCatalogo>();

    public DbSet<AbogadoCatalogo> Abogados =>
    Set<AbogadoCatalogo>();

    public DbSet<DiligenciaEncargadaCatalogo>
    DiligenciasEncargadas =>
    Set<DiligenciaEncargadaCatalogo>();

    public DbSet<DiligenciaRealizadaCatalogo>
    DiligenciasRealizadas =>
    Set<DiligenciaRealizadaCatalogo>();

    public DbSet<Documento> Documentos => Set<Documento>();

    public DbSet<Resolucion> Resoluciones => Set<Resolucion>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<UsuarioReceptor> UsuariosReceptores =>
    Set<UsuarioReceptor>();

    public DbSet<SesionUsuario> SesionesUsuario =>
    Set<SesionUsuario>();

    public DbSet<TokenRecuperacionPassword>
    TokensRecuperacionPassword => Set<TokenRecuperacionPassword>();

    public DbSet<AuditoriaSeguridad> AuditoriasSeguridad =>
    Set<AuditoriaSeguridad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JurigestDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // Con asincrono = false todas las consultas son síncronas: la tarea ya está completa.
        ResolverReceptoresAsync(asincrono: false, CancellationToken.None).GetAwaiter().GetResult();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await ResolverReceptoresAsync(asincrono: true, cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // Todas las rutas asignan el receptor por nombre; aquí se vincula al receptor del
    // catálogo para que la visibilidad no dependa del texto (p. ej. al renombrarlo).
    private async Task ResolverReceptoresAsync(bool asincrono, CancellationToken cancellationToken)
    {
        ChangeTracker.DetectChanges();

        var receptoresCambiados = ChangeTracker.Entries<ReceptorJudicialCatalogo>()
            .Where(e => e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(x => x.Nombre).IsModified))
            .Select(e => e.Entity)
            .ToList();

        // Diligencias con texto libre que coincide con un receptor creado o renombrado ahora.
        foreach (var receptor in receptoresCambiados)
        {
            var nombre = receptor.Nombre.Trim();
            var consulta = Diligencias.Where(d => d.ReceptorJudicialId == null && d.ReceptorJudicial == nombre);
            var libres = asincrono ? await consulta.ToListAsync(cancellationToken) : consulta.ToList();
            foreach (var diligencia in libres)
                Entry(diligencia).Property(x => x.ReceptorJudicialId).CurrentValue = receptor.Id;
        }

        var pendientes = ChangeTracker.Entries<Diligencia>()
            .Where(e => e.State == EntityState.Added ||
                (e.State == EntityState.Modified && e.Property(x => x.ReceptorJudicial).IsModified))
            .ToList();
        if (pendientes.Count == 0) return;

        var nombres = pendientes
            .Select(e => e.Entity.ReceptorJudicial?.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var consultaCatalogo = ReceptoresJudiciales.AsNoTracking()
            .Where(r => nombres.Contains(r.Nombre))
            .Select(r => new { r.Id, r.Nombre });
        var catalogo = (asincrono
                ? await consultaCatalogo.ToListAsync(cancellationToken)
                : consultaCatalogo.ToList())
            .ToDictionary(r => r.Id, r => r.Nombre);

        // Los receptores renombrados en esta misma operación se buscan por su nombre nuevo.
        foreach (var receptor in ChangeTracker.Entries<ReceptorJudicialCatalogo>())
        {
            if (receptor.State == EntityState.Deleted) catalogo.Remove(receptor.Entity.Id);
            else catalogo[receptor.Entity.Id] = receptor.Entity.Nombre;
        }

        var porNombre = catalogo
            .GroupBy(r => r.Value.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Key, StringComparer.OrdinalIgnoreCase);

        foreach (var entrada in pendientes)
        {
            var nombre = entrada.Entity.ReceptorJudicial?.Trim();
            entrada.Property(x => x.ReceptorJudicialId).CurrentValue =
                nombre is not null && porNombre.TryGetValue(nombre, out var id) ? id : null;
        }
    }
}
