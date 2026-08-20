using System.ComponentModel.DataAnnotations;

namespace Jurigest.Web.Models;

public sealed class RegistrarGestionModel : IValidatableObject
{
    [Required(ErrorMessage = "El ROL es obligatorio.")]
    [StringLength(30, ErrorMessage = "El ROL no puede superar 30 caracteres.")]
    [RegularExpression("^[^/\\\\?%*:|\"<>]+$", ErrorMessage = "El ROL contiene caracteres no permitidos.")]
    public string Rol { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tribunal es obligatorio.")]
    [StringLength(200)]
    public string Tribunal { get; set; } = string.Empty;

    [Required(ErrorMessage = "El abogado es obligatorio.")]
    [StringLength(200)]
    public string Abogado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El demandante es obligatorio.")]
    [StringLength(200)]
    public string Demandante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El demandado es obligatorio.")]
    [StringLength(200)]
    public string Demandado { get; set; } = string.Empty;

    [Required(ErrorMessage = "La diligencia es obligatoria.")]
    [StringLength(1000)]
    public string Diligencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de encargo es obligatoria.")]
    public DateTime? FechaEncargo { get; set; }

    public DateTime? FechaRetiro { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaEncargo.HasValue &&
            FechaRetiro.HasValue &&
            FechaRetiro.Value.Date < FechaEncargo.Value.Date)
        {
            yield return new ValidationResult(
                "La fecha de retiro debe ser igual o posterior a la fecha de encargo.",
                [nameof(FechaRetiro)]);
        }
    }
}
