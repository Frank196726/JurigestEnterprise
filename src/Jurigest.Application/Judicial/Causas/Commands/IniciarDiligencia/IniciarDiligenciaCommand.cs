using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.IniciarDiligencia;

public sealed record IniciarDiligenciaCommand(Guid Id) : IRequest<bool>;