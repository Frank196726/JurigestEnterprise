namespace Jurigest.Web.Models;

public sealed record TramoAntiguedadResumen(int Hasta10Dias, int Entre11Y15Dias, int MasDe15Dias);
public sealed record EstadoPlazosResumen(int Vencidas, int VencenHoy, int Proximos3Dias, int Entre4Y7Dias, int Posteriores);
public sealed record PanelEjecutivoResumen(int TotalCausas, TramoAntiguedadResumen CausasPorAntiguedad, EstadoPlazosResumen DiligenciasPorPlazo, DateTime GeneradoEnUtc);
public sealed record CausaEncontradaResumen(Guid Id, string Rit, string Tribunal, string Descripcion, DateTime FechaCreacion);
