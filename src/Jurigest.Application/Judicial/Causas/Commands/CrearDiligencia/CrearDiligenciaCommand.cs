using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Diligencias.Commands.CrearDiligencia;

public sealed record CrearDiligenciaCommand(
    Guid CausaId,
    string Descripcion,
    TipoDiligencia Tipo)
    : IRequest<Guid>;
