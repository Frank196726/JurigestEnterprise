using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class AvalSolidario
{
    private AvalSolidario() { }

    internal AvalSolidario(Guid demandadoId, string nombre, TipoPersonaDemandada tipoPersona,
        string? rut = null, string? representanteLegal = null, string? rutRepresentanteLegal = null)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 200)
            throw new ArgumentException("El nombre o razón social del aval debe tener entre 1 y 200 caracteres.", nameof(nombre));
        if (!Enum.IsDefined(tipoPersona))
            throw new ArgumentException("El tipo de persona del aval no es válido.", nameof(tipoPersona));
        Id = Guid.NewGuid();
        DemandadoId = demandadoId;
        Nombre = nombre.Trim();
        TipoPersona = tipoPersona;
        ActualizarIdentificacion(rut, representanteLegal, rutRepresentanteLegal);
    }

    public Guid Id { get; private set; }
    public Guid DemandadoId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public TipoPersonaDemandada TipoPersona { get; private set; }
    public string? Rut { get; private set; }
    public string? RepresentanteLegal { get; private set; }
    public string? RutRepresentanteLegal { get; private set; }

    public void ActualizarIdentificacion(string? rut, string? representanteLegal, string? rutRepresentanteLegal)
    {
        (Rut, RepresentanteLegal, RutRepresentanteLegal) =
            IdentificacionParte.Normalizar(TipoPersona, rut, representanteLegal, rutRepresentanteLegal);
    }
}
