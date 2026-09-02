using System.Net;
using System.Text.Json;
using Jurigest.Integration.Tests.Infrastructure;
namespace Jurigest.Integration.Tests.Judicial;
public sealed class ReportesTests
{
 [Fact] public async Task Reporte_SinAutenticacion_Devuelve401(){await using var f=new JurigestApiFactory();using var r=await f.CreateClient().GetAsync("/api/Causas/reporte");Assert.Equal(HttpStatusCode.Unauthorized,r.StatusCode);}
 [Fact] public async Task Reporte_ConsolidaCausaYDiligencias(){await using var f=new JurigestApiFactory();using var c=f.CreateClient();await SeguridadTestHelper.CrearAdministradorAsync(c);var a=await SeguridadTestHelper.IniciarSesionAsync(c,SeguridadTestHelper.AdminEmail,SeguridadTestHelper.AdminPassword);using var cc=await SeguridadTestHelper.EnviarAutorizadoAsync(c,HttpMethod.Post,"/api/Causas",a.Token,new{id=Guid.Empty,rit="C-REP-001-2026",tribunal="Tribunal Reporte",descripcion="Reporte consolidado",fechaEncargoCausa=DateTime.UtcNow.Date.AddDays(-5)});cc.EnsureSuccessStatusCode();var id=await GuidAsync(cc);using var cd=await SeguridadTestHelper.EnviarAutorizadoAsync(c,HttpMethod.Post,$"/api/Causas/{id}/diligencias",a.Token,new{descripcion="Diligencia reportable"});cd.EnsureSuccessStatusCode();using var r=await SeguridadTestHelper.EnviarAutorizadoAsync(c,HttpMethod.Get,"/api/Causas/reporte",a.Token);r.EnsureSuccessStatusCode();using var j=JsonDocument.Parse(await r.Content.ReadAsStringAsync());var fila=j.RootElement.EnumerateArray().Single(x=>x.GetProperty("id").GetGuid()==id);Assert.Equal(1,fila.GetProperty("totalDiligencias").GetInt32());Assert.Equal("Tribunal Reporte",fila.GetProperty("tribunal").GetString());}
 private static async Task<Guid> GuidAsync(HttpResponseMessage r){using var j=JsonDocument.Parse(await r.Content.ReadAsStringAsync());return j.RootElement.GetProperty("id").GetGuid();}
}
