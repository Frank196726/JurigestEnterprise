using MediatR;

namespace Jurigest.Application.Judicial.Panel.Queries.ObtenerPanelEjecutivo;

public sealed record ObtenerPanelEjecutivoQuery
    : IRequest<ObtenerPanelEjecutivoResponse>;
