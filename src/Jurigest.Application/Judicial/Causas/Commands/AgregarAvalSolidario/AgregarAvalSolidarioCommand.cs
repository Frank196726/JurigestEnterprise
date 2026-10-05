using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.AgregarAvalSolidario;

public sealed record AgregarAvalSolidarioCommand(
    Guid CausaId, Guid DemandadoId, string Nombre, TipoPersonaDemandada TipoPersona,
    string? Rut, string? RepresentanteLegal, string? RutRepresentanteLegal) : IRequest<Guid?>;

public sealed class AgregarAvalSolidarioHandler(ICausaRepository repository)
    : IRequestHandler<AgregarAvalSolidarioCommand, Guid?>
{
    public async Task<Guid?> Handle(AgregarAvalSolidarioCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rut))
            throw new ArgumentException("El RUT del aval solidario es obligatorio.", nameof(request.Rut));
        var causa = await repository.GetByIdAsync(request.CausaId, cancellationToken);
        var demandado = causa?.Demandados.FirstOrDefault(x => x.Id == request.DemandadoId);
        if (demandado is null) return null;

        var aval = demandado.AgregarAval(request.Nombre, request.TipoPersona,
            request.Rut, request.RepresentanteLegal, request.RutRepresentanteLegal);
        await repository.AddAvalSolidarioAsync(aval, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return aval.Id;
    }
}
