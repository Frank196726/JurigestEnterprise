using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Seguridad.Entities;
using Microsoft.EntityFrameworkCore;
using Jurigest.Domain.Judicial.Catalogos;

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
}

