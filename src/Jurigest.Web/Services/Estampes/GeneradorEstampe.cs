using System.Text;
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
        string? observaciones = null)
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

        if (string.IsNullOrWhiteSpace(resultadoDetalle))
        {
            throw new ArgumentException(
                "Debe indicar el resultado de la diligencia.",
                nameof(resultadoDetalle));
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

        var texto = new StringBuilder();

        texto.AppendLine(ObtenerTitulo(tipo));
        texto.AppendLine();

        texto.AppendLine($"ROL: {rit.Trim()}");
        texto.AppendLine($"TRIBUNAL: {tribunal.Trim()}");
        texto.AppendLine($"CARÁTULA: {caratula.Trim()}");
        texto.AppendLine();

        texto.AppendLine(
            $"FECHA DE DILIGENCIA: {fechaGestion:dd-MM-yyyy HH:mm}");

        if (!string.IsNullOrWhiteSpace(receptorJudicial))
        {
            texto.AppendLine(
                $"RECEPTOR JUDICIAL: {receptorJudicial.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(direccion))
        {
            texto.AppendLine(
                $"DIRECCIÓN: {direccion.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(comuna))
        {
            texto.AppendLine(
                $"COMUNA: {comuna.Trim()}");
        }

        texto.AppendLine();

        texto.AppendLine(
		$"DILIGENCIA REALIZADA: {diligenciaRealizada.Trim()}");

	texto.AppendLine(
		$"RESULTADO DE LA DILIGENCIA: {ObtenerNombreResultado(resultado)}");

	texto.AppendLine();

	texto.AppendLine(
		$"CERTIFICO: {resultadoDetalle.Trim()}");

        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            texto.AppendLine();
            texto.AppendLine(
                $"OBSERVACIONES: {observaciones.Trim()}");
        }

        return texto
            .ToString()
            .Trim();
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
