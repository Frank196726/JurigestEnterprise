using System.Text;
using System.Globalization;
using Jurigest.Domain.Judicial.Enums;

namespace Jurigest.Web.Services.Estampes;

public static class GeneradorEstampe
{
    public static string Generar(
        string rit,
        string tribunal,
        string caratula,
        string diligencia,
        TipoDiligencia tipo,
        string diligenciaRealizada,
        ResultadoDiligencia resultado,
        string resultadoDetalle,
        DateTime fechaGestion,
        string? receptorJudicial = null,
        string? direccion = null,
        string? comuna = null,
        string? observaciones = null,
        string? modelo = null,
        decimal? monto = null,
        string? abogado = null,
        string? rutDemandado = null,
        string? materia = null)
    {
        if (string.IsNullOrWhiteSpace(rit))
        {
            throw new ArgumentException(
                "El ROL es obligatorio.",
                nameof(rit));
        }

        if (string.IsNullOrWhiteSpace(tribunal))
        {
            throw new ArgumentException(
                "El tribunal es obligatorio.",
                nameof(tribunal));
        }

        if (string.IsNullOrWhiteSpace(caratula))
        {
            throw new ArgumentException(
                "La carátula es obligatoria.",
                nameof(caratula));
        }

        if (string.IsNullOrWhiteSpace(diligencia))
        {
            throw new ArgumentException(
                "La diligencia es obligatoria.",
                nameof(diligencia));
        }

        if (resultado == ResultadoDiligencia.SinResultado)
        {
            throw new ArgumentException(
                "Debe indicar la clasificación del resultado.",
                nameof(resultado));
        }

        if (string.IsNullOrWhiteSpace(diligenciaRealizada))
        {
            throw new ArgumentException(
                "La diligencia realizada es obligatoria.",
                nameof(diligenciaRealizada));
        }

        if (fechaGestion == default)
        {
            throw new ArgumentException(
                "Debe indicar la fecha de gestión.",
                nameof(fechaGestion));
        }

        string? contenidoModelo = null;
        if (!string.IsNullOrWhiteSpace(modelo))
        {
            var valores = new Dictionary<string, string>
            {
                ["{{TITULO}}"] = ObtenerTitulo(tipo),
                ["{{ROL}}"] = rit.Trim(),
                ["{{TRIBUNAL}}"] = tribunal.Trim(),
                ["{{CARATULA}}"] = caratula.Trim(),
                ["{{FECHA_GESTION}}"] = fechaGestion.ToString("dd-MM-yyyy HH:mm"),
                ["{{RECEPTOR}}"] = receptorJudicial?.Trim() ?? string.Empty,
                ["{{DIRECCION}}"] = direccion?.Trim() ?? string.Empty,
                ["{{COMUNA}}"] = comuna?.Trim() ?? string.Empty,
                ["{{DILIGENCIA_REALIZADA}}"] = diligenciaRealizada.Trim(),
                ["{{CLASIFICACION}}"] = ObtenerNombreResultado(resultado),
                ["{{RESULTADO_DETALLE}}"] = resultadoDetalle?.Trim() ?? string.Empty,
                ["{{ABOGADO}}"] = abogado?.Trim() ?? string.Empty,
                ["{{RUT_DEMANDADO}}"] = rutDemandado?.Trim() ?? string.Empty,
                ["{{MATERIA}}"] = materia?.Trim() ?? string.Empty,
                ["{{OBSERVACIONES}}"] = observaciones?.Trim() ?? string.Empty
            };
            contenidoModelo = modelo.Trim();
            foreach (var valor in valores)
                contenidoModelo = contenidoModelo.Replace(valor.Key, valor.Value, StringComparison.Ordinal);

            var partesCaratula = caratula.Split('/', 2, StringSplitOptions.TrimEntries);
            var nombreEjecutado = partesCaratula.Length == 2 ? partesCaratula[1] : caratula.Trim();
            var cultura = CultureInfo.GetCultureInfo("es-CL");
            var variablesLegadas = new Dictionary<string, string>
            {
                ["$fecha_palabras_diligencia"] = fechaGestion.ToString("dddd d 'de' MMMM 'de' yyyy", cultura),
                ["$hora_diligencia"] = fechaGestion.ToString("HH:mm", cultura),
                ["$direccion_ejecutado"] = direccion?.Trim() ?? string.Empty,
                ["$nombre_ejecutado"] = nombreEjecutado,
                ["$comuna_ejecutado"] = comuna?.Trim() ?? string.Empty,
                ["$rut_ejecutado"] = rutDemandado?.Trim() ?? string.Empty,
                ["$receptor_judicial"] = receptorJudicial?.Trim() ?? string.Empty
            };
            foreach (var variable in variablesLegadas)
                contenidoModelo = contenidoModelo.Replace(variable.Key, variable.Value, StringComparison.OrdinalIgnoreCase);

            if (contenidoModelo.StartsWith("CERTIFICO:", StringComparison.OrdinalIgnoreCase))
                contenidoModelo = contenidoModelo["CERTIFICO:".Length..].TrimStart();
            contenidoModelo = contenidoModelo.Trim();
        }

        var texto = new StringBuilder();

        texto.AppendLine(receptorJudicial?.Trim() ?? string.Empty);
        texto.AppendLine("Receptor Judicial");
        texto.AppendLine(new string('-', 72));
        texto.AppendLine();
        texto.AppendLine($"Tribunal: {tribunal.Trim()}");
        texto.AppendLine($"ROL: {rit.Trim()}");
        texto.AppendLine($"Caratulado: {caratula.Trim()}");
        texto.AppendLine($"Abogado: {abogado?.Trim() ?? string.Empty}");
        texto.AppendLine($"Materia: {materia?.Trim() ?? string.Empty}");
        texto.AppendLine($"DILIGENCIA REALIZADA: {diligenciaRealizada.Trim()}");
        texto.AppendLine($"Rut Demandado: {rutDemandado?.Trim() ?? string.Empty}");
        texto.AppendLine();

        texto.AppendLine("CERTIFICO:");

        if (!string.IsNullOrWhiteSpace(contenidoModelo))
        {
            texto.AppendLine();
            texto.AppendLine(contenidoModelo);
        }

        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            texto.AppendLine();
            texto.AppendLine(
                $"OBSERVACIONES: {observaciones.Trim()}");
        }

        var estampe = texto.ToString().Trim();
        return monto.HasValue ? Jurigest.Domain.Judicial.MontoEstampe.Aplicar(estampe, monto.Value) : estampe;
    }

    private static string ObtenerTitulo(
        TipoDiligencia tipo)
    {
        return tipo switch
        {
            TipoDiligencia.Notificacion =>
                "ESTAMPE DE NOTIFICACIÓN",

            TipoDiligencia.RequerimientoPago =>
                "ESTAMPE DE REQUERIMIENTO DE PAGO",

            TipoDiligencia.Embargo =>
                "ESTAMPE DE EMBARGO",

            TipoDiligencia.Lanzamiento =>
                "ESTAMPE DE LANZAMIENTO",

            TipoDiligencia.RetiroExhorto =>
                "ESTAMPE DE RETIRO DE EXHORTO",

            TipoDiligencia.RetiroExpediente =>
                "ESTAMPE DE RETIRO DE EXPEDIENTE",

            TipoDiligencia.Incautacion =>
                "ESTAMPE DE INCAUTACIÓN",

            TipoDiligencia.Protesto =>
                "ESTAMPE DE PROTESTO",

            TipoDiligencia.Citacion =>
                "ESTAMPE DE CITACIÓN",

            _ =>
                "ESTAMPE DE DILIGENCIA"
        };
    }

    private static string ObtenerNombreDiligencia(
    TipoDiligencia tipo)
    {
        return tipo switch
        {
            TipoDiligencia.Notificacion =>
                "NOTIFICACIÓN",

            TipoDiligencia.RequerimientoPago =>
                "REQUERIMIENTO DE PAGO",

            TipoDiligencia.Embargo =>
                "EMBARGO",

            TipoDiligencia.Lanzamiento =>
                "LANZAMIENTO",

            TipoDiligencia.RetiroExhorto =>
                "RETIRO DE EXHORTO",

            TipoDiligencia.RetiroExpediente =>
                "RETIRO DE EXPEDIENTE",

            TipoDiligencia.Incautacion =>
                "INCAUTACIÓN",

            TipoDiligencia.Protesto =>
                "PROTESTO",

            TipoDiligencia.Citacion =>
                "CITACIÓN",

            _ =>
                "OTRA DILIGENCIA"
        };
    }

    private static string ObtenerNombreResultado(
        ResultadoDiligencia resultado)
    {
        return resultado switch
        {
            ResultadoDiligencia.Positiva =>
                "POSITIVA",

            ResultadoDiligencia.Negativa =>
                "NEGATIVA",

            _ =>
                "SIN RESULTADO"
        };
    }
}
