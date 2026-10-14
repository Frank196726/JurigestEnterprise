using Jurigest.Domain.Judicial.Entities;
using Jurigest.Domain.Judicial.Enums;
using Jurigest.Persistence.Context;
using Jurigest.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jurigest.Integration.Tests.Judicial;

public sealed class PagoReciboPersistenciaTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PagarConservaCorrelativoSinIncluirloEnLaActualizacion(bool desconectado)
    {
        var options = new DbContextOptionsBuilder<JurigestDbContext>()
            .UseInMemoryDatabase($"PagoRecibo-{Guid.NewGuid()}")
            .Options;
        await using var db = new JurigestDbContext(options);
        var fecha = new DateTime(2026, 10, 14, 10, 0, 0);
        var recibo = new Recibo(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "Notificación de prueba", 25000, fecha);
        db.Recibos.Add(recibo);
        db.Entry(recibo).Property(x => x.Numero).CurrentValue = 42;
        await db.SaveChangesAsync();
        if (desconectado)
        {
            db.ChangeTracker.Clear();
        }

        // SQL Server rechaza cualquier UPDATE de una columna IDENTITY,
        // incluso cuando se intenta escribir el mismo valor.
        db.SavingChanges += (_, _) =>
            Assert.False(db.Entry(recibo).Property(x => x.Numero).IsModified);
        recibo.MarcarPagado(fecha.AddHours(1));
        await new ReciboRepository(db).UpdateAsync(recibo, CancellationToken.None);

        db.ChangeTracker.Clear();
        var guardado = await db.Recibos.SingleAsync();
        Assert.Equal(42, guardado.Numero);
        Assert.Equal(25000, guardado.Monto);
        Assert.Equal(EstadoRecibo.Pagado, guardado.Estado);
        Assert.Equal(fecha.AddHours(1), guardado.FechaPago);
    }
}
