namespace Jurigest.API.Contracts;

/// <summary>
/// Solicitud que puede asignar un receptor judicial a una diligencia.
/// <see cref="Security.AccesoPorReceptorAttribute"/> valida que un usuario con
/// receptores asociados solo asigne los suyos.
/// </summary>
public interface IAsignaReceptorRequest
{
    string? ReceptorJudicial { get; }
}
