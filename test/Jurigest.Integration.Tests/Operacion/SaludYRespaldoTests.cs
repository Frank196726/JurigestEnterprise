using System.Net;
using Jurigest.Integration.Tests.Infrastructure;
namespace Jurigest.Integration.Tests.Operacion;
public sealed class SaludYRespaldoTests
{
 [Fact] public async Task Salud_EstaDisponibleSinAutenticacion(){await using var f=new JurigestApiFactory();using var c=f.CreateClient();Assert.Equal(HttpStatusCode.OK,(await c.GetAsync("/health/live")).StatusCode);Assert.Equal(HttpStatusCode.OK,(await c.GetAsync("/health/ready")).StatusCode);}
 [Fact] public async Task Respaldo_RequiereAdministradorYGeneraJson(){await using var f=new JurigestApiFactory();using var c=f.CreateClient();Assert.Equal(HttpStatusCode.Unauthorized,(await c.GetAsync("/api/operacion/respaldo-judicial")).StatusCode);await SeguridadTestHelper.CrearAdministradorAsync(c);var a=await SeguridadTestHelper.IniciarSesionAsync(c,SeguridadTestHelper.AdminEmail,SeguridadTestHelper.AdminPassword);using var r=await SeguridadTestHelper.EnviarAutorizadoAsync(c,HttpMethod.Get,"/api/operacion/respaldo-judicial",a.Token);r.EnsureSuccessStatusCode();Assert.Equal("application/json",r.Content.Headers.ContentType?.MediaType);Assert.Contains("attachment",r.Content.Headers.ContentDisposition?.DispositionType);Assert.Contains("causas",await r.Content.ReadAsStringAsync());}
}
