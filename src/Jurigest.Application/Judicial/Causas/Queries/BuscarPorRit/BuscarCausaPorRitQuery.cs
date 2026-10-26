using Jurigest.Application.Judicial;
using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

public sealed record BuscarCausaPorRitQuery(
    string Rit,
    FiltroReceptores? Receptores = null)
    : IRequest<BuscarCausaPorRitResponse?>;
