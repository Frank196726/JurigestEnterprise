using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.AgregarDemandado;

public sealed record AgregarDemandadoCommand(
    Guid CausaId, string Nombre, TipoPersonaDemandada TipoPersona, bool EsPrincipal,
    string? Rut, string? RepresentanteLegal, string? RutRepresentanteLegal) : IRequest<Guid?>;

public sealed class AgregarDemandadoHandler(ICausaRepository repository)
    : IRequestHandler<AgregarDemandadoCommand, Guid?>
{
    public async Task<Guid?> Handle(AgregarDemandadoCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rut))
            throw new ArgumentException("El RUT del demandado es obligatorio.", nameof(request.Rut));
        var causa = await repository.GetByIdAsync(request.CausaId, cancellationToken);
        if (causa is null) return null;

        var demandado = causa.AgregarDemandado(request.Nombre, request.TipoPersona, request.EsPrincipal,
            request.Rut, request.RepresentanteLegal, request.RutRepresentanteLegal);
        await repository.AddDemandadoAsync(demandado, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return demandado.Id;
    }
}
