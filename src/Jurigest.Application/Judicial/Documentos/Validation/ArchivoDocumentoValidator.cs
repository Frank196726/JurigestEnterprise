using System.IO.Compression;

namespace Jurigest.Application.Judicial.Documentos.Validation;

public static class ArchivoDocumentoValidator
{
    private static readonly byte[] FirmaPdf = "%PDF-"u8.ToArray();

    private static readonly byte[] FirmaDoc =
    [
        0xD0, 0xCF, 0x11, 0xE0,
        0xA1, 0xB1, 0x1A, 0xE1
    ];

    public static async Task<string> ValidarAsync(
        Stream contenido,
        string nombreArchivo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        if (!contenido.CanSeek)
        {
            throw new ArgumentException(
                "No se puede validar el contenido del archivo.");
        }

        var posicionOriginal = contenido.Position;
        var extension = Path
            .GetExtension(nombreArchivo)
            .ToLowerInvariant();

        try
        {
            contenido.Position = 0;

            return extension switch
            {
                ".pdf" when await ComienzaConAsync(
                    contenido,
                    FirmaPdf,
                    cancellationToken) => "application/pdf",

                ".doc" when await ComienzaConAsync(
                    contenido,
                    FirmaDoc,
                    cancellationToken) => "application/msword",

                ".docx" when EsDocumentoWordOpenXml(contenido) =>
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                ".pdf" or ".doc" or ".docx" =>
                    throw new ArgumentException(
                        "El contenido no coincide con el tipo de archivo."),

                _ => throw new ArgumentException(
                    "Solo se permiten archivos PDF, DOC y DOCX.")
            };
        }
        finally
        {
            contenido.Position = posicionOriginal;
        }
    }

    private static async Task<bool> ComienzaConAsync(
        Stream contenido,
        byte[] firma,
        CancellationToken cancellationToken)
    {
        contenido.Position = 0;

        var buffer = new byte[firma.Length];
        var leidos = await contenido.ReadAsync(
            buffer.AsMemory(0, buffer.Length),
            cancellationToken);

        return leidos == firma.Length &&
               buffer.AsSpan().SequenceEqual(firma);
    }

    private static bool EsDocumentoWordOpenXml(Stream contenido)
    {
        contenido.Position = 0;

        try
        {
            using var zip = new ZipArchive(
                contenido,
                ZipArchiveMode.Read,
                leaveOpen: true);

            return zip.GetEntry("[Content_Types].xml") is not null &&
                   zip.GetEntry("word/document.xml") is not null;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}