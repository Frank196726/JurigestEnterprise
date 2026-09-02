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
        : this(
            rit,
            tribunal,
            descripcion,
            DateTime.UtcNow)
    {
    }

    public Causa(
        string rit,
        string tribunal,
        string descripcion,
        DateTime fechaEncargoCausa)
    {
        ValidarRit(rit);
        ValidarTribunal(tribunal);
        ValidarDescripcion(descripcion);
        ValidarFechaEncargo(fechaEncargoCausa);

        Id = Guid.NewGuid();

        Rit = rit.Trim();

        Tribunal = tribunal.Trim();

        Descripcion = descripcion.Trim();

        FechaCreacion = DateTime.UtcNow;

        FechaEncargoCausa = fechaEncargoCausa;

        // Una causa nueva todavía no ha sido gestionada.
        FechaGestionCausa = null;

        Estado = EstadoCausa.Ingresada;
    }

    public Guid Id { get; private set; }

    public string Rit { get; private set; } =
        string.Empty;

    public Guid? TipoCausaId { get; private set; }

    public string? NumeroRol { get; private set; }

    public string Tribunal { get; private set; } =
        string.Empty;

    public string Descripcion { get; private set; } =
        string.Empty;

    public DateTime FechaCreacion { get; private set; }

    /// <summary>
    /// Fecha en que la causa fue encargada originalmente
    /// para su gestión.
    /// </summary>
    public DateTime FechaEncargoCausa { get; private set; }

    /// <summary>
    /// Fecha de la última gestión efectivamente realizada.
    ///
    /// Permanece null mientras la causa no haya sido
    /// gestionada.
    /// </summary>
    public DateTime? FechaGestionCausa { get; private set; }

    public EstadoCausa Estado { get; private set; }

    public IReadOnlyCollection<Diligencia> Diligencias =>
        _diligencias.AsReadOnly();

    public void AsignarTipoCausa(
        Guid tipoCausaId,
        string numeroRol,
        string codigoTipoCausa)
    {
        if (tipoCausaId == Guid.Empty)
        {
            throw new ArgumentException(
                "El tipo de causa es obligatorio.",
                nameof(tipoCausaId));
        }

        if (string.IsNullOrWhiteSpace(numeroRol))
        {
            throw new ArgumentException(
                "El número de ROL es obligatorio.",
                nameof(numeroRol));
        }

        if (numeroRol.IndexOfAny(
                CaracteresInvalidosRit) >= 0)
        {
            throw new ArgumentException(
                "El número de ROL contiene caracteres no permitidos.",
                nameof(numeroRol));
        }

        if (string.IsNullOrWhiteSpace(
                codigoTipoCausa))
        {
            throw new ArgumentException(
                "El código del tipo de causa es obligatorio.",
                nameof(codigoTipoCausa));
        }

        TipoCausaId =
            tipoCausaId;

        NumeroRol =
            numeroRol.Trim();

        Rit =
            $"{codigoTipoCausa.Trim().ToUpperInvariant()}-" +
            $"{NumeroRol}";
    }

    public void ActualizarDatos(
        string tribunal,
        string descripcion)
    {
        ValidarTribunal(tribunal);
        ValidarDescripcion(descripcion);

        Tribunal =
            tribunal.Trim();

        Descripcion =
            descripcion.Trim();
    }

    public void ActualizarDatos(
        string rit,
        string tribunal,
        string descripcion)
    {
        ValidarRit(rit);
        ValidarTribunal(tribunal);
        ValidarDescripcion(descripcion);

        Rit =
            rit.Trim();

        Tribunal =
            tribunal.Trim();

        Descripcion =
            descripcion.Trim();
    }

    /// <summary>
    /// Permite corregir la fecha original de encargo
    /// de la causa.
    /// </summary>
    public void ActualizarFechaEncargo(
        DateTime fechaEncargoCausa)
    {
        ValidarFechaEncargo(
            fechaEncargoCausa);

        if (FechaGestionCausa.HasValue &&
            fechaEncargoCausa >
            FechaGestionCausa.Value)
        {
            throw new ArgumentException(
                "La fecha de encargo no puede ser posterior " +
                "a la fecha de gestión de la causa.",
                nameof(fechaEncargoCausa));
        }

        FechaEncargoCausa =
            fechaEncargoCausa;
    }

    /// <summary>
    /// Registra una gestión efectivamente realizada
    /// sobre la causa.
    /// </summary>
    public void RegistrarGestion(
        DateTime fechaGestion)
    {
        if (fechaGestion == default)
        {
            throw new ArgumentException(
                "La fecha de gestión es obligatoria.",
                nameof(fechaGestion));
        }

        if (fechaGestion < FechaEncargoCausa)
        {
            throw new ArgumentException(
                "La fecha de gestión no puede ser anterior " +
                "a la fecha de encargo de la causa.",
                nameof(fechaGestion));
        }

        if (FechaGestionCausa.HasValue &&
            fechaGestion < FechaGestionCausa.Value)
        {
            throw new ArgumentException(
                "La nueva fecha de gestión no puede ser anterior " +
                "a la última gestión registrada.",
                nameof(fechaGestion));
        }

        FechaGestionCausa =
            fechaGestion;
    }

    /// <summary>
    /// Indica si la causa todavía no registra
    /// ninguna gestión efectiva.
    /// </summary>
    public bool EstaSinGestion =>
        !FechaGestionCausa.HasValue;

    /// <summary>
    /// Fecha utilizada como base para calcular
    /// los días sin gestión.
    /// </summary>
    public DateTime ObtenerFechaBaseGestion()
    {
        return FechaGestionCausa
            ?? FechaEncargoCausa;
    }

    /// <summary>
    /// Calcula los días transcurridos desde
    /// la última gestión o, si nunca fue gestionada,
    /// desde la fecha de encargo.
    /// </summary>
    public int ObtenerDiasSinGestion(
        DateTime fechaReferencia)
    {
        var fechaBase =
            ObtenerFechaBaseGestion().Date;

        var referencia =
            fechaReferencia.Date;

        if (referencia <= fechaBase)
        {
            return 0;
        }

        return
            (referencia - fechaBase).Days;
    }

    /// <summary>
    /// Una causa se considera crítica cuando supera
    /// 10 días sin gestión.
    /// </summary>
    public bool EstaCritica(
        DateTime fechaReferencia)
    {
        return ObtenerDiasSinGestion(
            fechaReferencia) > 10;
    }

    public Diligencia AgregarDiligencia(
        string descripcion)
    {
        if (string.IsNullOrWhiteSpace(
                descripcion))
        {
            throw new ArgumentException(
                "La descripción es obligatoria.",
                nameof(descripcion));
        }

        var diligencia =
            new Diligencia(
                Guid.NewGuid(),
                Id,
                descripcion);

        _diligencias.Add(
            diligencia);

        return diligencia;
    }

    public void EliminarDiligencia(
        Guid diligenciaId)
    {
        var diligencia =
            _diligencias.FirstOrDefault(
                d => d.Id == diligenciaId);

        if (diligencia is not null)
        {
            _diligencias.Remove(
                diligencia);
        }
    }

    private static void ValidarRit(
        string rit)
    {
        if (string.IsNullOrWhiteSpace(rit))
        {
            throw new ArgumentException(
                "El RIT es obligatorio.",
                nameof(rit));
        }

        if (rit.IndexOfAny(
                CaracteresInvalidosRit) >= 0)
        {
            throw new ArgumentException(
                "El RIT contiene caracteres no permitidos.",
                nameof(rit));
        }
    }

    private static void ValidarTribunal(
        string tribunal)
    {
        if (string.IsNullOrWhiteSpace(
                tribunal))
        {
            throw new ArgumentException(
                "El Tribunal es obligatorio.",
                nameof(tribunal));
        }
    }

    private static void ValidarDescripcion(
        string descripcion)
    {
        if (string.IsNullOrWhiteSpace(
                descripcion))
        {
            throw new ArgumentException(
                "La descripción es obligatoria.",
                nameof(descripcion));
        }
    }

    private static void ValidarFechaEncargo(
        DateTime fechaEncargoCausa)
    {
        if (fechaEncargoCausa == default)
        {
            throw new ArgumentException(
                "La fecha de encargo de la causa es obligatoria.",
                nameof(fechaEncargoCausa));
        }
    }
}
