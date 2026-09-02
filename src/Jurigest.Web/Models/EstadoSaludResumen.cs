namespace Jurigest.Web.Models;
public sealed record EstadoSaludResumen(string Estado, string? Servicio, string? BaseDatos, double? DuracionMs, DateTime HoraUtc);
