using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace Jurigest.Web.Components.Pages;

public partial class Documentos
{
    private const long MaximoArchivoBytes =
        10 * 1024 * 1024;

    private IBrowserFile? _archivoSeleccionado;
    private string _nombreDocumento = string.Empty;
    private int _tipoDocumento = 6;
    private string? _mensajeCarga;
    private string? _errorCarga;
    private bool _subiendo;

    private void SeleccionarArchivo(
        InputFileChangeEventArgs args)
    {
        _archivoSeleccionado = args.File;
        _mensajeCarga = null;
        _errorCarga = null;

        if (string.IsNullOrWhiteSpace(_nombreDocumento))
        {
            _nombreDocumento =
                Path.GetFileNameWithoutExtension(
                    args.File.Name);
        }
    }

    private async Task CargarArchivoAsync()
    {
        if (_subiendo)
            return;

        _mensajeCarga = null;
        _errorCarga = null;

        if (_causaSeleccionadaId == Guid.Empty ||
            _archivoSeleccionado is null ||
            string.IsNullOrWhiteSpace(_nombreDocumento))
        {
            _errorCarga =
                "Debe indicar causa, nombre y archivo.";

            return;
        }

        if (_archivoSeleccionado.Size >
            MaximoArchivoBytes)
        {
            _errorCarga =
                "El archivo supera el límite de 10 MB.";

            return;
        }

        var extension =
            Path.GetExtension(
                    _archivoSeleccionado.Name)
                .ToLowerInvariant();

        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" =>
                "application/vnd.openxmlformats-officedocument" +
                ".wordprocessingml.document",
            _ => null
        };

        if (contentType is null)
        {
            _errorCarga =
                "Solo se permiten archivos PDF, DOC y DOCX.";

            return;
        }

        _subiendo = true;

        try
        {
            await using var archivoStream =
                _archivoSeleccionado.OpenReadStream(
                    MaximoArchivoBytes);

            using var archivoContenido =
                new StreamContent(archivoStream);

            archivoContenido.Headers.ContentType =
                new MediaTypeHeaderValue(contentType);

            using var formulario =
                new MultipartFormDataContent();

            formulario.Add(
                new StringContent(
                    _nombreDocumento.Trim()),
                "Nombre");

            formulario.Add(
                new StringContent(
                    _tipoDocumento.ToString(
                        CultureInfo.InvariantCulture)),
                "Tipo");

            formulario.Add(
                archivoContenido,
                "Archivo",
                _archivoSeleccionado.Name);

            using var response =
                await ApiClient.PostAsync(
                    $"/api/Causas/{_causaSeleccionadaId}" +
                    "/documentos/archivo",
                    formulario);

            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                _errorCarga =
                    "Su usuario no tiene permiso para " +
                    "cargar documentos.";

                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadFromJsonAsync<MensajeResponse>();

                _errorCarga =
                    error?.Mensaje ??
                    "No fue posible cargar el documento.";

                return;
            }

            _mensajeCarga =
                "Documento cargado correctamente.";

            _archivoSeleccionado = null;
            _nombreDocumento = string.Empty;
            _tipoDocumento = 6;

            await CargarDocumentosAsync();
        }
        catch (UnauthorizedAccessException)
        {
            Navigation.NavigateTo(
                "/login",
                forceLoad: true);
        }
        catch (HttpRequestException)
        {
            _errorCarga =
                "No fue posible conectar con el servicio.";
        }
        catch (IOException)
        {
            _errorCarga =
                "No fue posible leer el archivo seleccionado.";
        }
        finally
        {
            _subiendo = false;
        }
    }

    private sealed record MensajeResponse(
        string Mensaje);
}