using MediatR;

namespace Jurigest.Application.Judicial.Causas.Commands.ActualizarCausa;

public sealed class ActualizarCausaCommand
    : IRequest<ActualizarCausaResponse?>
{
    public Guid Id { get; set; }

    public string Tribunal { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime? FechaEncargoCausa { get; set; }

    public Guid? DiligenciaId { get; set; }

    public Guid? DiligenciaEncargadaId { get; set; }

    public DateTime? FechaProgramada { get; set; }
}