namespace Jurigest.Web.Models;

public sealed record ReporteCausaResumen(
    Guid Id,
    string Rit,
    string Tribunal,
    string Descripcion,
    DateTime FechaEncargo,
    DateTime? UltimaGestion,
    int DiasSinGestion,
    int Estado,
    string Responsables,
    int TotalDiligencias,
    int DiligenciasCompletadas);
