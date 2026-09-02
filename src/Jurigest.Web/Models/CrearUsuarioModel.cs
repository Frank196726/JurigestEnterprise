using System.ComponentModel.DataAnnotations;

namespace Jurigest.Web.Models;

public sealed class CrearUsuarioModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(10, ErrorMessage = "La contraseña debe tener al menos 10 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Range(1, 4)]
    public int Rol { get; set; } = 3;
}
