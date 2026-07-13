namespace Jurigest.Domain.Kernel.Results;

/// <summary>
/// Representa un error del dominio o de la aplicación.
/// </summary>
public sealed record Error(string Code, string Description)
{
    /// <summary>
    /// Representa la ausencia de error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}