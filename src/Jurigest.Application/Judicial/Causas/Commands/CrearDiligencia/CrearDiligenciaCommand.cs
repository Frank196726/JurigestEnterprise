using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CrearDiligencia;

public sealed record CrearDiligenciaCommand(
    Guid CausaId,
    string Descripcion)
    : IRequest<Guid>;