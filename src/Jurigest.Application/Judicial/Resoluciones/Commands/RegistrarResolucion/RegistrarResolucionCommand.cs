using Jurigest.Domain.Judicial.Enums;
using MediatR;

namespace Jurigest.Application.Judicial.Resoluciones.Commands.RegistrarResolucion;

public sealed record RegistrarResolucionCommand(
    Guid CausaId,
    TipoResolucion Tipo,
    DateTime Fecha,
    string Descripcion)
    : IRequest<Guid?>;