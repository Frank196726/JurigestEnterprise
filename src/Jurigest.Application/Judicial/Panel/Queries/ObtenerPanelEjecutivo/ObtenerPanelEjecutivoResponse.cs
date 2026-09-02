namespace Jurigest.Application.Judicial.Panel.Queries.ObtenerPanelEjecutivo;

public sealed record TramoAntiguedadResponse(
    int Hasta10Dias,
    int Entre11Y15Dias,
    int MasDe15Dias);

public sealed record EstadoPlazosResponse(
    int Vencidas,
    int VencenHoy,
    int Proximos3Dias,
    int Entre4Y7Dias,
    int Posteriores);

public sealed record ObtenerPanelEjecutivoResponse(
    int TotalCausas,
    TramoAntiguedadResponse CausasPorAntiguedad,
    EstadoPlazosResponse DiligenciasPorPlazo,
    DateTime GeneradoEnUtc);
