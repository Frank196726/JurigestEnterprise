namespace Jurigest.Web.Models;

public sealed record DocumentoResumen(
    Guid Id,
    Guid CausaId,
    string Nombre,
    int Tipo,
    string RutaArchivo,
    string ContentType,
    long TamanoBytes,
    DateTime FechaRegistro);