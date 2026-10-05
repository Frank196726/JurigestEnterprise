using System.ComponentModel.DataAnnotations;

namespace Jurigest.Web.Models;

public sealed class CrearUsuarioModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El RUT es obligatorio.")]
    public string Rut { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El N° de oficina es obligatorio.")]
    public string NumeroOficina { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(12, ErrorMessage = "La contraseña debe tener al menos 12 caracteres.")]
    public string Password { get; set; } = string.Empty;

    public Guid? RolCatalogoId { get; set; }

    [Range(1, 4)]
    public int Rol { get; set; } = 3;
}
