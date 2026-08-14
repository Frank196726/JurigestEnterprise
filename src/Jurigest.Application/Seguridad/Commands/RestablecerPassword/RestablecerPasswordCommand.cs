using MediatR;

namespace Jurigest.Application.Seguridad.Commands.RestablecerPassword;

public sealed record RestablecerPasswordCommand(
    string Email,
    string NuevaPassword)
    : IRequest<bool>;