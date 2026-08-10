using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.RegistrarCoordenadas;

public sealed record RegistrarCoordenadasCommand(
    Guid Id,
    decimal Latitud,
    decimal Longitud)
    : IRequest<bool>;
