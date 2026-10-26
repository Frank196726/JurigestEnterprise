using Jurigest.Application.Judicial;
using MediatR;

namespace Jurigest.Application.Judicial.Panel.Queries.ObtenerPanelEjecutivo;

public sealed record ObtenerPanelEjecutivoQuery(FiltroReceptores? Receptores = null)
    : IRequest<ObtenerPanelEjecutivoResponse>;
