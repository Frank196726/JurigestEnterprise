using System.Net.Http.Headers;
using Jurigest.Web.Security;
namespace Jurigest.Web.Endpoints;
public static class OperacionWebEndpoints
{
 public static IEndpointRouteBuilder MapOperacionWebEndpoints(this IEndpointRouteBuilder endpoints){endpoints.MapGet("/sistema/respaldo",DescargarAsync).RequireAuthorization();return endpoints;}
 private static async Task<IResult> DescargarAsync(HttpContext context,IHttpClientFactory factory,ISesionWebStore store,CancellationToken ct)
 {
  if(!context.Request.Cookies.TryGetValue(SeguridadWebEndpoints.CookieName,out var id)||string.IsNullOrWhiteSpace(id)||store.Obtener(id)is not{} sesion)return Results.Unauthorized();
  var client=factory.CreateClient("JurigestApi");using var request=new HttpRequestMessage(HttpMethod.Get,"/api/operacion/respaldo-judicial");request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",sesion.AccessToken);using var response=await client.SendAsync(request,ct);if(response.StatusCode==System.Net.HttpStatusCode.Forbidden)return Results.Forbid();if(!response.IsSuccessStatusCode)return Results.StatusCode((int)response.StatusCode);var bytes=await response.Content.ReadAsByteArrayAsync(ct);var nombre=response.Content.Headers.ContentDisposition?.FileNameStar??$"jurigest-respaldo-{DateTime.Today:yyyyMMdd}.json";return Results.File(bytes,"application/json",nombre.Trim('"'));
 }
}
