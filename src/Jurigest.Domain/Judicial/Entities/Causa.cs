using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class Causa
{
    private static readonly char[] CaracteresInvalidosRit =
        ['/', '\\', '?', '%', '*', ':', '|', '"', '<', '>'];

    private readonly List<Diligencia> _diligencias = new();

    private Causa()
    {
    }

    public Causa(
        string rit,
        string tribunal,
        string descripcion)
    {
        if (string.IsNullOrWhiteSpace(rit))
            throw new ArgumentException("El RIT es obligatorio.");

        if (rit.IndexOfAny(CaracteresInvalidosRit) >= 0)
            throw new ArgumentException("El RIT contiene caracteres no permitidos.");

        if (string.IsNullOrWhiteSpace(tribunal))
            throw new ArgumentException("El Tribunal es obligatorio.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción es obligatoria.");

        Id = Guid.NewGuid();
        Rit = rit.Trim();
        Tribunal = tribunal.Trim();
        Descripcion = descripcion.Trim();
        FechaCreacion = DateTime.UtcNow;
        Estado = EstadoCausa.Ingresada;
    }

    public Guid Id { get; private set; }

    public string Rit { get; private set; } = string.Empty;

    public string Tribunal { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public DateTime FechaCreacion { get; private set; }

    public EstadoCausa Estado { get; private set; }

    public IReadOnlyCollection<Diligencia> Diligencias
        => _diligencias.AsReadOnly();

    public void ActualizarDatos(
        string tribunal,
        string descripcion)
    {
        if (string.IsNullOrWhiteSpace(tribunal))
            throw new ArgumentException("El Tribunal es obligatorio.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción es obligatoria.");

        Tribunal = tribunal.Trim();
        Descripcion = descripcion.Trim();
    }

    public void ActualizarDatos(
        string rit,
        string tribunal,
        string descripcion)
    {
        if (string.IsNullOrWhiteSpace(rit))
            throw new ArgumentException("El RIT es obligatorio.");

        if (rit.IndexOfAny(CaracteresInvalidosRit) >= 0)
            throw new ArgumentException("El RIT contiene caracteres no permitidos.");

        if (string.IsNullOrWhiteSpace(tribunal))
            throw new ArgumentException("El Tribunal es obligatorio.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción es obligatoria.");

        Rit = rit.Trim();
        Tribunal = tribunal.Trim();
        Descripcion = descripcion.Trim();
    }

    public Diligencia AgregarDiligencia(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción es obligatoria.");

        var diligencia = new Diligencia(
            Guid.NewGuid(),
            Id,
            descripcion);

        _diligencias.Add(diligencia);

        return diligencia;
    }

    public void EliminarDiligencia(Guid diligenciaId)
    {
        var diligencia = _diligencias.FirstOrDefault(d => d.Id == diligenciaId);

        if (diligencia is not null)
            _diligencias.Remove(diligencia);
    }
}
