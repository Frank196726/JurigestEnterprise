using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Jurigest.Web.Models;
using Jurigest.Web.Security;

namespace Jurigest.Web.Endpoints;

public static class ReportesWebEndpoints
{
    public static IEndpointRouteBuilder MapReportesWebEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/reportes/causas.csv", ExportarCausasAsync)
            .RequireAuthorization();
        return endpoints;
    }

    private static async Task<IResult> ExportarCausasAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        ISesionWebStore sesionStore,
        CancellationToken cancellationToken)
    {
        if (!context.Request.Cookies.TryGetValue(SeguridadWebEndpoints.CookieName, out var sesionId) ||
            string.IsNullOrWhiteSpace(sesionId) || sesionStore.Obtener(sesionId) is not { } sesion)
            return Results.Unauthorized();

        var client = httpClientFactory.CreateClient("JurigestApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sesion.AccessToken);
        var causas = await client.GetFromJsonAsync<List<ReporteCausaResumen>>("/api/Causas/reporte", cancellationToken) ?? [];
        var desde = ParseDate(context.Request.Query["desde"]);
        var hasta = ParseDate(context.Request.Query["hasta"]);
        var estado = int.TryParse(context.Request.Query["estado"], out var estadoValor) ? estadoValor : 0;
        var tribunal = context.Request.Query["tribunal"].ToString();
        var responsable = context.Request.Query["responsable"].ToString();
        var filas = new List<ReporteCausaResumen>();

        foreach (var causa in causas.Where(c => (!desde.HasValue || c.FechaEncargo.Date >= desde.Value.Date) && (!hasta.HasValue || c.FechaEncargo.Date <= hasta.Value.Date) && (estado == 0 || c.Estado == estado) && (string.IsNullOrWhiteSpace(tribunal) || c.Tribunal.Equals(tribunal, StringComparison.OrdinalIgnoreCase))))
        {
            if (!string.IsNullOrWhiteSpace(responsable) && !causa.Responsables.Contains(responsable, StringComparison.OrdinalIgnoreCase)) continue;
            filas.Add(causa);
        }

        var csv = new StringBuilder("RIT;Tribunal;Descripción;Fecha encargo;Última gestión;Días sin gestionar;Estado;Responsables;Diligencias;Completadas\r\n");
        foreach (var fila in filas)
            csv.AppendLine(string.Join(';', Q(fila.Rit), Q(fila.Tribunal), Q(fila.Descripcion), fila.FechaEncargo.ToString("dd-MM-yyyy"), fila.UltimaGestion?.ToString("dd-MM-yyyy HH:mm") ?? "", fila.DiasSinGestion, Q(Estado(fila.Estado)), Q(fila.Responsables), fila.TotalDiligencias, fila.DiligenciasCompletadas));

        return Results.File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv; charset=utf-8", $"reporte-causas-{DateTime.Today:yyyyMMdd}.csv");
    }

    private static DateTime? ParseDate(string? value) => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha) ? fecha : null;
    private static string Q(object? value) => $"\"{value?.ToString()?.Replace("\"", "\"\"")}\"";
    private static string Estado(int value) => value switch { 1 => "Ingresada", 2 => "En tramitación", 3 => "Suspendida", 4 => "Terminada", 5 => "Archivada", _ => "Sin estado" };
}
