using MediatR;
namespace Jurigest.Application.Judicial.Reportes.Queries.ObtenerReporteCausas;
public sealed record ObtenerReporteCausasQuery : IRequest<List<ReporteCausaResponse>>;
public sealed record ReporteCausaResponse(Guid Id,string Rit,string Tribunal,string Descripcion,DateTime FechaEncargo,DateTime? UltimaGestion,int DiasSinGestion,int Estado,string Responsables,int TotalDiligencias,int DiligenciasCompletadas);
