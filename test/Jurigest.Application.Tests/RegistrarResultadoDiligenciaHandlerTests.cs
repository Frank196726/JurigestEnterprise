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
                catalogoRepository,
                new ReciboRepositoryFake());

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
                catalogoRepository,
                new ReciboRepositoryFake());

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
                catalogoRepository,
                new ReciboRepositoryFake());

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
                    diligenciaRealizada),
                new ReciboRepositoryFake());

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
                new DiligenciaRealizadaCatalogoRepositoryFake(null),
                new ReciboRepositoryFake());

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
                    diligenciaRealizada),
                new ReciboRepositoryFake());

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

    [Fact]
    public async Task Handle_NotificacionPersonalConArancel_GeneraReciboPendiente()
    {
        var fechaEncargo =
            new DateTime(
                2026, 9, 20, 9, 0, 0,
                DateTimeKind.Utc);

        var fechaGestion =
            fechaEncargo.AddDays(1);

        var causa =
            new Causa(
                "C-RECIBO-1-2026",
                "1° Juzgado Civil",
                "Persona A / Persona B",
                fechaEncargo);

        var diligencia =
            causa.AgregarDiligencia(
                "Notificación");

        var diligenciaRealizada =
            new DiligenciaRealizadaCatalogo(
                Guid.NewGuid(),
                "Notificación personal",
                (int)diligencia.Tipo);

        diligenciaRealizada.CambiarArancel(60000);

        var causaRepository =
            new CausaRepositoryFake(causa);

        var reciboRepository =
            new ReciboRepositoryFake();

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                new DiligenciaRepositoryFake(diligencia),
                causaRepository,
                new DiligenciaRealizadaCatalogoRepositoryFake(
                    diligenciaRealizada),
                reciboRepository);

        await handler.Handle(
            new RegistrarResultadoDiligenciaCommand(
                diligencia.Id,
                diligenciaRealizada.Id,
                ResultadoDiligencia.Positiva,
                "Notificación entregada",
                "Estampe receptor",
                fechaGestion),
            CancellationToken.None);

        var recibo =
            Assert.Single(
                reciboRepository.RecibosAgregados);

        Assert.Equal(
            causa.Id,
            recibo.CausaId);

        Assert.Equal(
            diligencia.Id,
            recibo.DiligenciaId);

        Assert.Equal(
            diligenciaRealizada.Id,
            recibo.DiligenciaRealizadaId);

        Assert.Equal(
            "Notificación personal",
            recibo.DiligenciaRealizada);

        Assert.Equal(
            60000m,
            recibo.Monto);

        Assert.Equal(
            EstadoRecibo.Pendiente,
            recibo.Estado);

        Assert.Equal(
            fechaGestion,
            recibo.FechaEmision);

        Assert.Null(
            recibo.FechaPago);

        Assert.Equal(
            1,
            causaRepository.SaveChangesCalls);
    }
    [Fact]
    public async Task Handle_DiligenciaConReciboExistente_NoGeneraDuplicado()
    {
        var fechaEncargo =
            new DateTime(
                2026, 9, 20, 9, 0, 0,
                DateTimeKind.Utc);

        var fechaGestion =
            fechaEncargo.AddDays(1);

        var causa =
            new Causa(
                "C-RECIBO-2-2026",
                "1° Juzgado Civil",
                "Persona A / Persona B",
                fechaEncargo);

        var diligencia =
            causa.AgregarDiligencia(
                "Notificación");

        var diligenciaRealizada =
            new DiligenciaRealizadaCatalogo(
                Guid.NewGuid(),
                "Notificación personal",
                (int)diligencia.Tipo);

        diligenciaRealizada.CambiarArancel(60000);

        var reciboExistente =
            new Recibo(
                Guid.NewGuid(),
                causa.Id,
                diligencia.Id,
                diligenciaRealizada.Id,
                diligenciaRealizada.Nombre,
                60000m,
                fechaGestion.AddHours(-1));

        var causaRepository =
            new CausaRepositoryFake(causa);

        var reciboRepository =
            new ReciboRepositoryFake(
                reciboExistente);

        var handler =
            new RegistrarResultadoDiligenciaCommandHandler(
                new DiligenciaRepositoryFake(diligencia),
                causaRepository,
                new DiligenciaRealizadaCatalogoRepositoryFake(
                    diligenciaRealizada),
                reciboRepository);

        await handler.Handle(
            new RegistrarResultadoDiligenciaCommand(
                diligencia.Id,
                diligenciaRealizada.Id,
                ResultadoDiligencia.Positiva,
                "Notificación entregada",
                "Estampe receptor",
                fechaGestion),
            CancellationToken.None);

        Assert.Empty(
            reciboRepository.RecibosAgregados);

        Assert.Equal(
            1,
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

    private sealed class ReciboRepositoryFake(
        Recibo? recibo = null)
        : IReciboRepository
    {
        public List<Recibo> RecibosAgregados { get; } = new();

        public Task<Recibo?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                recibo?.Id == id
                    ? recibo
                    : RecibosAgregados.FirstOrDefault(x => x.Id == id));

        public Task<Recibo?> GetByDiligenciaIdAsync(
            Guid diligenciaId,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                recibo?.DiligenciaId == diligenciaId
                    ? recibo
                    : RecibosAgregados.FirstOrDefault(
                        x => x.DiligenciaId == diligenciaId));

        public Task<List<Recibo>> GetAllAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(
                RecibosAgregados.ToList());

        public Task AddAsync(
            Recibo reciboNuevo,
            CancellationToken cancellationToken)
        {
            RecibosAgregados.Add(reciboNuevo);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Recibo reciboActualizado,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}




