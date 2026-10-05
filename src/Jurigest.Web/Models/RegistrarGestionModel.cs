using System.ComponentModel.DataAnnotations;

namespace Jurigest.Web.Models;

public sealed class RegistrarGestionModel : IValidatableObject
{
    // Compatibilidad temporal con el modelo anterior.
    public string Rol { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "El tipo de causa es obligatorio.")]
    public Guid? TipoCausaId { get; set; }

    [Required(
        ErrorMessage = "El número de ROL es obligatorio.")]
    [StringLength(
        30,
        ErrorMessage =
            "El número de ROL no puede superar 30 caracteres.")]
    [RegularExpression(
        "^[^/\\\\?%*:|\"<>]+$",
        ErrorMessage =
            "El número de ROL contiene caracteres no permitidos.")]
    public string NumeroRol { get; set; } =
        string.Empty;

    // =========================================================
    // CATÁLOGOS
    // =========================================================

    [Required(
        ErrorMessage = "El tribunal es obligatorio.")]
    public Guid? TribunalId { get; set; }

    [Required(
        ErrorMessage = "El abogado responsable es obligatorio.")]
    public Guid? AbogadoId { get; set; }

    [Required(
        ErrorMessage = "La diligencia encargada es obligatoria.")]
    public Guid? DiligenciaEncargadaId { get; set; }

    // =========================================================
    // VALORES RESUELTOS
    //
    // Se mantienen porque la API actual todavía recibe texto.
    // =========================================================

    public string Tribunal { get; set; } =
        string.Empty;

    public string Abogado { get; set; } =
        string.Empty;

    public string Diligencia { get; set; } =
        string.Empty;

    // =========================================================
    // PARTES
    // =========================================================

    [Required(
        ErrorMessage = "El demandante es obligatorio.")]
    [StringLength(200)]
    public string Demandante { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "El demandado es obligatorio.")]
    [StringLength(200)]
    public string Demandado { get; set; } =
        string.Empty;

    [Required(ErrorMessage = "El primer apellido para la carátula es obligatorio.")]
    [StringLength(200)]
    public string DemandadoCaratula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione la materia del juicio.")]
    public Guid? MateriaId { get; set; }

    public int TipoPersonaDemandado { get; set; } = 1;
    [Required(ErrorMessage = "El RUT del demandado principal es obligatorio.")]
    public string RutDemandado { get; set; } = string.Empty;
    public string RepresentanteLegalDemandado { get; set; } = string.Empty;
    public string RutRepresentanteLegalDemandado { get; set; } = string.Empty;
    public List<DemandadoGestionModel> OtrosDemandados { get; set; } = [];
    public List<DemandadoGestionModel> AvalesSolidarios { get; set; } = [];

    [Required(
        ErrorMessage = "El domicilio del demandado es obligatorio.")]
    [StringLength(
        300,
        ErrorMessage =
            "El domicilio del demandado no puede superar 300 caracteres.")]
    public string Direccion { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "La comuna es obligatoria.")]
    [StringLength(
        150,
        ErrorMessage =
            "La comuna no puede superar 150 caracteres.")]
    public string Comuna { get; set; } =
        string.Empty;

    // =========================================================
    // FECHAS
    // =========================================================

    [Required(
        ErrorMessage = "La fecha de encargo de la causa es obligatoria.")]
    public DateTime? FechaEncargoCausa { get; set; }

    [Required(
        ErrorMessage = "La fecha programada de la diligencia es obligatoria.")]
    public DateTime? FechaProgramada { get; set; }

    public DateTime? FechaRetiro { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        foreach (var persona in OtrosDemandados.Concat(AvalesSolidarios))
        {
            if (string.IsNullOrWhiteSpace(persona.Nombre) || persona.Nombre.Trim().Length > 200)
                yield return new ValidationResult("Cada demandado y aval debe tener un nombre o razón social de hasta 200 caracteres.");
            if (persona.TipoPersona is not (1 or 2))
                yield return new ValidationResult("Seleccione un tipo de persona válido.");
            if (string.IsNullOrWhiteSpace(persona.Rut))
                yield return new ValidationResult("Cada demandado y aval debe tener RUT.");
            else
            {
                if (!EsRutValido(persona.Rut)) yield return new ValidationResult($"El RUT de {persona.Nombre} no es válido.");
            }
            if (persona.TipoPersona == 1 && (!string.IsNullOrWhiteSpace(persona.RepresentanteLegal) || !string.IsNullOrWhiteSpace(persona.RutRepresentanteLegal)))
                yield return new ValidationResult("El representante legal corresponde solo a personas jurídicas.");
            if (!string.IsNullOrWhiteSpace(persona.RutRepresentanteLegal))
            {
                if (!EsRutValido(persona.RutRepresentanteLegal)) yield return new ValidationResult($"El RUT del representante de {persona.Nombre} no es válido.");
            }
        }
        if (TipoPersonaDemandado is not (1 or 2))
            yield return new ValidationResult("Seleccione un tipo de persona válido.", [nameof(TipoPersonaDemandado)]);
        if (!string.IsNullOrWhiteSpace(RutDemandado))
        {
            if (!EsRutValido(RutDemandado)) yield return new ValidationResult("El RUT del demandado principal no es válido.", [nameof(RutDemandado)]);
        }
        if (TipoPersonaDemandado == 1 && (!string.IsNullOrWhiteSpace(RepresentanteLegalDemandado) || !string.IsNullOrWhiteSpace(RutRepresentanteLegalDemandado)))
            yield return new ValidationResult("El representante legal corresponde solo a personas jurídicas.");
        if (!string.IsNullOrWhiteSpace(RutRepresentanteLegalDemandado))
        {
            if (!EsRutValido(RutRepresentanteLegalDemandado)) yield return new ValidationResult("El RUT del representante legal no es válido.");
        }
        if (FechaEncargoCausa.HasValue &&
            FechaRetiro.HasValue &&
            FechaRetiro.Value.Date <
            FechaEncargoCausa.Value.Date)
        {
            yield return new ValidationResult(
                "La fecha de retiro debe ser igual o posterior a la fecha de encargo de la causa.",
                [nameof(FechaRetiro)]);
        }

        if (FechaEncargoCausa.HasValue &&
            FechaProgramada.HasValue &&
            FechaProgramada.Value.Date <
            FechaEncargoCausa.Value.Date)
        {
            yield return new ValidationResult(
                "La fecha programada de la diligencia no puede ser anterior a la fecha de encargo de la causa.",
                [nameof(FechaProgramada)]);

        }
    }
    private static bool EsRutValido(string rut)
    {
        try { Jurigest.Domain.Judicial.RutChileno.Normalizar(rut); return true; }
        catch (ArgumentException) { return false; }
    }
}
