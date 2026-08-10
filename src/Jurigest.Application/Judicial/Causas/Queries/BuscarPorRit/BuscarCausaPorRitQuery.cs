using MediatR;

namespace Jurigest.Application.Judicial.Causas.Queries.BuscarPorRit;

public sealed record BuscarCausaPorRitQuery(
    string Rit)
    : IRequest<BuscarCausaPorRitResponse?>;