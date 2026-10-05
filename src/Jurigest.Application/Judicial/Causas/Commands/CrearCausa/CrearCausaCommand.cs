using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.CrearCausa;

public sealed class CrearCausaCommand
    : IRequest<CrearCausaResponse>
{
    public Guid Id { get; init; }

    public string Rit { get; init; } = string.Empty;

    public Guid? TipoCausaId { get; init; }

    public string NumeroRol { get; init; } =
        string.Empty;

    public string Tribunal { get; init; } =
        string.Empty;

    public string Descripcion { get; init; } =
        string.Empty;

    public Guid? MateriaId { get; init; }
    public string? Materia { get; init; }
    public DateTime FechaEncargoCausa { get; init; }
    public List<DemandadoInput> Demandados { get; init; } = [];
}
