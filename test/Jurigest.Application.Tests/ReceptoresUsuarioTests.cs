using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Application.Seguridad;
using Jurigest.Domain.Judicial.Catalogos;

namespace Jurigest.Application.Tests;

public sealed class ReceptoresUsuarioTests
{
    private static readonly ReceptorJudicialCatalogo Activo = new(Guid.NewGuid(), "Activo");

    [Fact]
    public async Task ValidarAsync_ConservaAsociacionesActualesAReceptoresInactivos()
    {
        var inactivo = Guid.NewGuid();
        var quitado = Activo.Id;

        var ids = await ReceptoresUsuario.ValidarAsync(
            [Activo.Id], new CatalogoFake(), CancellationToken.None, [inactivo, quitado]);

        Assert.Equal([Activo.Id, inactivo], ids);
    }

    [Fact]
    public async Task ValidarAsync_RechazaListaVaciaOReceptorInactivo()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ReceptoresUsuario.ValidarAsync([], new CatalogoFake(), CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ReceptoresUsuario.ValidarAsync([Guid.NewGuid()], new CatalogoFake(), CancellationToken.None));
    }

    private sealed class CatalogoFake : IReceptorJudicialCatalogoRepository
    {
        public Task<List<ReceptorJudicialCatalogo>> GetActivosAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new List<ReceptorJudicialCatalogo> { Activo });

        public Task<ReceptorJudicialCatalogo?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task AddAsync(ReceptorJudicialCatalogo receptor, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
