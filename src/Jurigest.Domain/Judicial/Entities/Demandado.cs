using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class Demandado
{
    private readonly List<AvalSolidario> _avales = [];
    private Demandado() { }

    internal Demandado(Guid causaId, string nombre, TipoPersonaDemandada tipoPersona, bool esPrincipal,
        string? rut = null, string? representanteLegal = null, string? rutRepresentanteLegal = null)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 200)
            throw new ArgumentException("El nombre o razón social del demandado debe tener entre 1 y 200 caracteres.", nameof(nombre));
        if (!Enum.IsDefined(tipoPersona))
            throw new ArgumentException("El tipo de persona del demandado no es válido.", nameof(tipoPersona));
        Id = Guid.NewGuid();
        CausaId = causaId;
        Nombre = nombre.Trim();
        TipoPersona = tipoPersona;
        EsPrincipal = esPrincipal;
        ActualizarIdentificacion(rut, representanteLegal, rutRepresentanteLegal);
    }

    public Guid Id { get; private set; }
    public Guid CausaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public TipoPersonaDemandada TipoPersona { get; private set; }
    public bool EsPrincipal { get; private set; }
    public string? Rut { get; private set; }
    public string? RepresentanteLegal { get; private set; }
    public string? RutRepresentanteLegal { get; private set; }
    public IReadOnlyCollection<AvalSolidario> Avales => _avales.AsReadOnly();

    public void ActualizarIdentificacion(string? rut, string? representanteLegal, string? rutRepresentanteLegal)
    {
        (Rut, RepresentanteLegal, RutRepresentanteLegal) =
            IdentificacionParte.Normalizar(TipoPersona, rut, representanteLegal, rutRepresentanteLegal);
    }

    public AvalSolidario AgregarAval(string nombre, TipoPersonaDemandada tipoPersona,
        string? rut = null, string? representanteLegal = null, string? rutRepresentanteLegal = null)
    {
        if (!EsPrincipal)
            throw new InvalidOperationException("Solo el demandado principal puede tener avales solidarios.");
        var aval = new AvalSolidario(Id, nombre, tipoPersona, rut, representanteLegal, rutRepresentanteLegal);
        if (_avales.Any(x => string.Equals(x.Nombre, aval.Nombre, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("El aval solidario ya está registrado.", nameof(nombre));
        _avales.Add(aval);
        return aval;
    }
}
