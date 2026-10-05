using Jurigest.Application.Abstractions.Persistence;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.ActualizarIdentificacionParte;

public sealed record ActualizarIdentificacionParteCommand(
    Guid CausaId, Guid DemandadoId, Guid? AvalId,
    string Rut, string? RepresentanteLegal, string? RutRepresentanteLegal) : IRequest<bool>;

public sealed class ActualizarIdentificacionParteHandler(ICausaRepository repository)
    : IRequestHandler<ActualizarIdentificacionParteCommand, bool>
{
    public async Task<bool> Handle(ActualizarIdentificacionParteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rut))
            throw new ArgumentException("El RUT es obligatorio.", nameof(request.Rut));
        var causa = await repository.GetByIdAsync(request.CausaId, cancellationToken);
        var demandado = causa?.Demandados.FirstOrDefault(x => x.Id == request.DemandadoId);
        if (demandado is null) return false;

        if (request.AvalId.HasValue)
        {
            var aval = demandado.Avales.FirstOrDefault(x => x.Id == request.AvalId.Value);
            if (aval is null) return false;
            aval.ActualizarIdentificacion(request.Rut, request.RepresentanteLegal, request.RutRepresentanteLegal);
        }
        else
        {
            demandado.ActualizarIdentificacion(request.Rut, request.RepresentanteLegal, request.RutRepresentanteLegal);
        }
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
