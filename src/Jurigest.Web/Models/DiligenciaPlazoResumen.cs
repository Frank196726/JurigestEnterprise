namespace Jurigest.Web.Models;

public sealed record DiligenciaPlazoResumen(
    Guid Id,
    Guid CausaId,
    string Rit,
    string Tribunal,
    string Descripcion,
    int Estado,
    int Tipo,
    DateTime FechaProgramada,
    int DiasParaVencimiento,
    string? ReceptorJudicial);
