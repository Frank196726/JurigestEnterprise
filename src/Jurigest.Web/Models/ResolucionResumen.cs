namespace Jurigest.Web.Models;

public sealed record ResolucionResumen(
    Guid Id,
    Guid CausaId,
    int Tipo,
    DateTime Fecha,
    string Descripcion);
