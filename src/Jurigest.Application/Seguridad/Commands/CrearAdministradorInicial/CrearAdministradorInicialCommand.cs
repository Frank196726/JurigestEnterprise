using MediatR;

namespace Jurigest.Application.Seguridad.Commands.CrearAdministradorInicial;

public sealed record CrearAdministradorInicialCommand(
    string Nombre,
    string Email,
    string Password)
    : IRequest<Guid?>;