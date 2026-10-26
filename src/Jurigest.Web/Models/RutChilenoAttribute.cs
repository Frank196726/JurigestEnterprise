using System.ComponentModel.DataAnnotations;
using Jurigest.Domain.Judicial;

namespace Jurigest.Web.Models;

/// <summary>
/// Valida formato y dígito verificador del RUT con la misma regla del dominio,
/// para avisar en el formulario antes de enviar los datos a la API.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RutChilenoAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var rut = value as string;
        if (string.IsNullOrWhiteSpace(rut)) return ValidationResult.Success; // Lo exige [Required].

        try
        {
            RutChileno.Normalizar(rut);
            return ValidationResult.Success;
        }
        catch (ArgumentException)
        {
            return new ValidationResult(
                "El RUT no es válido: revisa el número y el dígito verificador (ej.: 12.345.678-5).",
                [validationContext.MemberName ?? "Rut"]);
        }
    }
}
