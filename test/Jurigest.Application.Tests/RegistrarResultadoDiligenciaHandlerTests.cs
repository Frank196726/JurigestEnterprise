using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Judicial.Diligencias.Commands.RegistrarResultado;
using Jurigest.Domain.Judicial.Catalogos;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Tests;

public sealed class RegistrarResultadoDiligenciaHandlerTests
{
    [Fact]
    public async Task Handle_RegistraResultadoYActualizaGestionDeCausa()
    {
        var fechaEncargo =
            new DateTime(
                2026, 8, 1, 9, 0, 0,
                DateTimeKind.Utc);

        var fechaGestion =
            fechaEncargo.AddDays(4);

        var causa =
            new Causa(
                "C-123-2026",
                "1° Juzgado Civil",
                "Persona A / Persona B",
                fechaEncargo);

        var diligencia =
            causa.AgregarDiligencia(
                "Notificación");

        var diligenciaRealizada =
            new DiligenciaRealizadaCatalogo(
                Guid.NewGuid(),
                "Notificación realizada",
                (int)diligencia.Tipo);

        var causaRepository =
            new CausaRepositoryFake(causa);

        var diligenciaRepository =
            new DiligenciaRepositoryFake(diligencia);

        var catalogoRepository =
            new DiligenciaRealizadaCatalogoRepositoryFake(
                diligenciaRealizada);

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                diligenciaRepository,
                causaRepository,
                catalogoRepository);

        await handler.Handle(
            new RegistrarResultadoDiligenciaCommand(
                diligencia.Id,
                diligenciaRealizada.Id,
                ResultadoDiligencia.Positiva,
                "Notificación entregada",
                "Estampe receptor",
                fechaGestion),
            CancellationToken.None);

        Assert.Equal(
            EstadoDiligencia.Completada,
            diligencia.Estado);

        Assert.Equal(
            diligenciaRealizada.Id,
            diligencia.DiligenciaRealizadaId);

        Assert.Equal(
            diligenciaRealizada.Nombre,
            diligencia.DiligenciaRealizada);

        Assert.Equal(
            fechaGestion,
            diligencia.FechaGestion);

        Assert.Equal(
            fechaGestion,
            causa.FechaGestionCausa);

        Assert.Equal(
            1,
            causaRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_DiligenciaInexistente_LanzaExcepcionSinGuardar()
    {
        var causaRepository =
            new CausaRepositoryFake(null);

        var diligenciaRepository =
            new DiligenciaRepositoryFake(null);

        var catalogoRepository =
            new DiligenciaRealizadaCatalogoRepositoryFake(null);

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                diligenciaRepository,
                causaRepository,
                catalogoRepository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(
                new RegistrarResultadoDiligenciaCommand(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    ResultadoDiligencia.Positiva,
                    "Detalle",
                    "Estampe",
                    DateTime.UtcNow),
                CancellationToken.None));

        Assert.Equal(
            0,
            causaRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_CausaAsociadaInexistente_LanzaExcepcionSinGuardar()
    {
        var diligencia =
            new Diligencia(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Notificación");

        var causaRepository =
            new CausaRepositoryFake(null);

        var catalogoRepository =
            new DiligenciaRealizadaCatalogoRepositoryFake(null);

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                new DiligenciaRepositoryFake(diligencia),
                causaRepository,
                catalogoRepository);

        var excepcion =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(
                    new RegistrarResultadoDiligenciaCommand(
                        diligencia.Id,
                        Guid.NewGuid(),
                        ResultadoDiligencia.Positiva,
                        "Detalle",
                        "Estampe",
                        DateTime.UtcNow),
                    CancellationToken.None));

        Assert.Equal(
            "La causa asociada a la diligencia no existe.",
            excepcion.Message);

        Assert.Equal(
            0,
            causaRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_ResultadoInvalido_NoActualizaCausaNiGuarda()
    {
        var fechaEncargo =
            new DateTime(
                2026, 8, 1, 9, 0, 0,
                DateTimeKind.Utc);

        var causa =
            new Causa(
                "C-123-2026",
                "1° Juzgado Civil",
                "Persona A / Persona B",
                fechaEncargo);

        var diligencia =
            causa.AgregarDiligencia(
                "Notificación");

        var diligenciaRealizada =
            new DiligenciaRealizadaCatalogo(
                Guid.NewGuid(),
                "Notificación realizada",
                (int)diligencia.Tipo);

        var causaRepository =
            new CausaRepositoryFake(causa);

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                new DiligenciaRepositoryFake(diligencia),
                causaRepository,
                new DiligenciaRealizadaCatalogoRepositoryFake(
                    diligenciaRealizada));

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(
                new RegistrarResultadoDiligenciaCommand(
                    diligencia.Id,
                    diligenciaRealizada.Id,
                    ResultadoDiligencia.SinResultado,
                    "Detalle",
                    "Estampe",
                    fechaEncargo.AddDays(1)),
                CancellationToken.None));

        Assert.Null(
            causa.FechaGestionCausa);

        Assert.Equal(
            0,
            causaRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_DiligenciaRealizadaInexistente_LanzaExcepcionSinGuardar()
    {
        var fechaEncargo =
            new DateTime(
                2026, 8, 1, 9, 0, 0,
                DateTimeKind.Utc);

        var causa =
            new Causa(
                "C-123-2026",
                "1° Juzgado Civil",
                "Persona A / Persona B",
                fechaEncargo);

        var diligencia =
            causa.AgregarDiligencia(
                "Notificación");

        var causaRepository =
            new CausaRepositoryFake(causa);

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                new DiligenciaRepositoryFake(diligencia),
                causaRepository,
                new DiligenciaRealizadaCatalogoRepositoryFake(null));

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(
                new RegistrarResultadoDiligenciaCommand(
                    diligencia.Id,
                    Guid.NewGuid(),
                    ResultadoDiligencia.Positiva,
                    "Detalle",
                    "Estampe",
                    fechaEncargo.AddDays(1)),
                CancellationToken.None));

        Assert.Null(
            causa.FechaGestionCausa);

        Assert.Equal(
            0,
            causaRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_DiligenciaRealizadaDeOtroTipo_LanzaExcepcionSinGuardar()
    {
        var fechaEncargo =
            new DateTime(
                2026, 8, 1, 9, 0, 0,
                DateTimeKind.Utc);

        var causa =
            new Causa(
                "C-123-2026",
                "1° Juzgado Civil",
                "Persona A / Persona B",
                fechaEncargo);

        var diligencia =
            causa.AgregarDiligencia(
                "Notificación");

        var diligenciaRealizada =
            new DiligenciaRealizadaCatalogo(
                Guid.NewGuid(),
                "Notificación realizada",
                (int)TipoDiligencia.Notificacion);

        Assert.NotEqual(
            (int)diligencia.Tipo,
            diligenciaRealizada.CodigoTipoDiligencia);

        var causaRepository =
            new CausaRepositoryFake(causa);

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                new DiligenciaRepositoryFake(diligencia),
                causaRepository,
                new DiligenciaRealizadaCatalogoRepositoryFake(
                    diligenciaRealizada));

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(
                new RegistrarResultadoDiligenciaCommand(
                    diligencia.Id,
                    diligenciaRealizada.Id,
                    ResultadoDiligencia.Positiva,
                    "Detalle",
                    "Estampe",
                    fechaEncargo.AddDays(1)),
                CancellationToken.None));

        Assert.Null(
            causa.FechaGestionCausa);

        Assert.Equal(
            0,
            causaRepository.SaveChangesCalls);
    }

    private sealed class CausaRepositoryFake(Causa? causa)
        : ICausaRepository
    {
        public int SaveChangesCalls { get; private set; }

        public Task<Causa?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                causa?.Id == id ? causa : null);

        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }

        public Task AddAsync(
            Causa value,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<List<Causa>> GetAllAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new List<Causa>());

        public Task UpdateAsync(
            Causa value,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                causa?.Id == id);

        public Task DeleteAsync(
            Causa value,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<Causa?> GetByRitAsync(
            string rit,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                causa?.Rit == rit ? causa : null);
    }

    private sealed class DiligenciaRepositoryFake(
        Diligencia? diligencia)
        : IDiligenciaRepository
    {
        public Task<Diligencia?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                diligencia?.Id == id
                    ? diligencia
                    : null);

        public Task<List<Diligencia>> GetAllAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new List<Diligencia>());

        public Task AddAsync(
            Diligencia value,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<List<Diligencia>> GetByCausaAsync(
            Guid causaId,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new List<Diligencia>());

        public Task UpdateAsync(
            Diligencia value,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteAsync(
            Diligencia value,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<List<Diligencia>> GetByCausaIdAsync(
            Guid causaId,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new List<Diligencia>());

        public Task<Diligencia?> GetUltimaByCausaAsync(
            Guid causaId,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                diligencia?.CausaId == causaId
                    ? diligencia
                    : null);
    }

    private sealed class DiligenciaRealizadaCatalogoRepositoryFake(
        DiligenciaRealizadaCatalogo? diligenciaRealizada)
        : IDiligenciaRealizadaCatalogoRepository
    {
        public Task<List<DiligenciaRealizadaCatalogo>> GetActivosAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(
                diligenciaRealizada is not null &&
                diligenciaRealizada.Activo
                    ? new List<DiligenciaRealizadaCatalogo>
                    {
                        diligenciaRealizada
                    }
                    : new List<DiligenciaRealizadaCatalogo>());

        public Task<List<DiligenciaRealizadaCatalogo>> GetActivosPorTipoAsync(
            int codigoTipoDiligencia,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                diligenciaRealizada is not null &&
                diligenciaRealizada.Activo &&
                diligenciaRealizada.CodigoTipoDiligencia ==
                    codigoTipoDiligencia
                    ? new List<DiligenciaRealizadaCatalogo>
                    {
                        diligenciaRealizada
                    }
                    : new List<DiligenciaRealizadaCatalogo>());

        public Task<DiligenciaRealizadaCatalogo?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                diligenciaRealizada?.Id == id
                    ? diligenciaRealizada
                    : null);

        public Task<bool> ExisteNombreAsync(
            string nombre,
            int codigoTipoDiligencia,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                diligenciaRealizada is not null &&
                diligenciaRealizada.Nombre == nombre &&
                diligenciaRealizada.CodigoTipoDiligencia ==
                    codigoTipoDiligencia);

        public Task AddAsync(
            DiligenciaRealizadaCatalogo diligencia,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
