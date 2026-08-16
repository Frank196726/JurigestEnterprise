using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CerrarSesion;

public sealed record CerrarSesionCommand(
    string RefreshToken)
    : IRequest;