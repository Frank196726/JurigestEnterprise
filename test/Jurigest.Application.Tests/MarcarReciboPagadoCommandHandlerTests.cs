using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Judicial.Recibos.Commands.MarcarPagado;
using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Application.Tests;

public sealed class MarcarReciboPagadoCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReciboPendiente_MarcaPagadoYConservaDatosHistoricos()
    {
        var reciboId = Guid.NewGuid();
        var causaId = Guid.NewGuid();
        var diligenciaId = Guid.NewGuid();
        var diligenciaRealizadaId = Guid.NewGuid();

        var fechaEmision =
            new DateTime(2026, 9, 20, 15, 30, 0);

        var fechaPago =
            new DateTime(2026, 9, 21, 10, 0, 0);

        var recibo = new Recibo(
            reciboId,
            causaId,
            diligenciaId,
            diligenciaRealizadaId,
            "Notificación personal",
            60000m,
            fechaEmision);

        var repository =
            new ReciboRepositoryFake(recibo);

        var handler =
            new MarcarReciboPagadoCommandHandler(
                repository);

        var resultado = await handler.Handle(
            new MarcarReciboPagadoCommand(
                reciboId,
                fechaPago),
            CancellationToken.None);

        Assert.True(resultado);

        Assert.Equal(
            EstadoRecibo.Pagado,
            recibo.Estado);

        Assert.Equal(
            fechaPago,
            recibo.FechaPago);

        Assert.Equal(60000m, recibo.Monto);
        Assert.Equal(causaId, recibo.CausaId);
        Assert.Equal(diligenciaId, recibo.DiligenciaId);

        Assert.Equal(
            diligenciaRealizadaId,
            recibo.DiligenciaRealizadaId);

        Assert.Equal(
            "Notificación personal",
            recibo.DiligenciaRealizada);

        Assert.Equal(
            fechaEmision,
            recibo.FechaEmision);

        Assert.Equal(
            1,
            repository.UpdateCalls);

        Assert.Same(
            recibo,
            repository.UltimoReciboActualizado);
    }

    [Fact]
    public async Task Handle_ReciboNoExiste_DevuelveFalseSinActualizar()
    {
        var repository =
            new ReciboRepositoryFake();

        var handler =
            new MarcarReciboPagadoCommandHandler(
                repository);

        var resultado = await handler.Handle(
            new MarcarReciboPagadoCommand(
                Guid.NewGuid(),
                new DateTime(2026, 9, 21, 10, 0, 0)),
            CancellationToken.None);

        Assert.False(resultado);
        Assert.Equal(0, repository.UpdateCalls);
        Assert.Null(repository.UltimoReciboActualizado);
    }

    private sealed class ReciboRepositoryFake(
        Recibo? recibo = null)
        : IReciboRepository
    {
        public int UpdateCalls { get; private set; }

        public Recibo? UltimoReciboActualizado
        {
            get;
            private set;
        }

        public Task<Recibo?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                recibo?.Id == id
                    ? recibo
                    : null);
        }

        public Task<Recibo?> GetByDiligenciaIdAsync(
            Guid diligenciaId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                recibo?.DiligenciaId == diligenciaId
                    ? recibo
                    : null);
        }

        public Task<List<Recibo>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            var resultado =
                recibo is null
                    ? new List<Recibo>()
                    : new List<Recibo> { recibo };

            return Task.FromResult(resultado);
        }

        public Task AddAsync(
            Recibo reciboNuevo,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Recibo reciboActualizado,
            CancellationToken cancellationToken)
        {
            UpdateCalls++;
            UltimoReciboActualizado =
                reciboActualizado;

            return Task.CompletedTask;
        }
    }
}
