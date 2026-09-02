using Jurigest.Application.Abstractions.Persistence;
using Jurigest.Domain.Judicial.Enums;
using MediatR;
namespace Jurigest.Application.Judicial.Reportes.Queries.ObtenerReporteCausas;
public sealed class ObtenerReporteCausasHandler(ICausaRepository causas, IDiligenciaRepository diligencias) : IRequestHandler<ObtenerReporteCausasQuery,List<ReporteCausaResponse>>
{
 public async Task<List<ReporteCausaResponse>> Handle(ObtenerReporteCausasQuery request,CancellationToken ct)
 {
  var hoy=DateTime.UtcNow.Date; var cs=await causas.GetAllAsync(ct); var ds=await diligencias.GetAllAsync(ct); var grupos=ds.GroupBy(x=>x.CausaId).ToDictionary(x=>x.Key,x=>x.ToList());
  return cs.Select(c=>{var g=grupos.GetValueOrDefault(c.Id)??[];var fechas=g.Select(x=>x.FechaGestion??x.FechaRealizada).Where(x=>x.HasValue).Select(x=>x!.Value);DateTime? ultima=fechas.Any()?fechas.Max():null;var referencia=ultima??c.FechaGestionCausa??c.FechaEncargoCausa;var responsables=string.Join(", ",g.Select(x=>x.ReceptorJudicial).Where(x=>!string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase));return new ReporteCausaResponse(c.Id,c.Rit,c.Tribunal,c.Descripcion,c.FechaEncargoCausa,ultima??c.FechaGestionCausa,Math.Max(0,(hoy-referencia.Date).Days),(int)c.Estado,responsables,g.Count,g.Count(x=>x.Estado==EstadoDiligencia.Completada));}).OrderByDescending(x=>x.DiasSinGestion).ThenBy(x=>x.Rit).ToList();
 }
}
