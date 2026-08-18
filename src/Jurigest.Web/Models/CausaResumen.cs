namespace Jurigest.Web.Models;

public sealed record CausaResumen(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaCreacion,
    int Estado);