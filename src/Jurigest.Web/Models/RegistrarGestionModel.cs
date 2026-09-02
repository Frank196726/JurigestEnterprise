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
}