namespace Jurigest.Application.Abstractions.Storage;

public interface IArchivoStorage
{
    Task<string> GuardarAsync(
        Stream contenido,
        string extension,
        CancellationToken cancellationToken);

    Task<Stream?> AbrirLecturaAsync(
        string rutaArchivo,
        CancellationToken cancellationToken);

    Task EliminarAsync(
        string rutaArchivo,
        CancellationToken cancellationToken);
}