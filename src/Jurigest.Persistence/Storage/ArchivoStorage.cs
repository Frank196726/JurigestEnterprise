using Jurigest.Application.Abstractions.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jurigest.Persistence.Storage;

public sealed class ArchivoStorage : IArchivoStorage
{
    private readonly string _directorioRaiz;

    public ArchivoStorage(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var rutaConfigurada =
            configuration["FileStorage:RootPath"]
            ?? "App_Data/Documentos";

        _directorioRaiz = Path.GetFullPath(
            Path.IsPathRooted(rutaConfigurada)
                ? rutaConfigurada
                : Path.Combine(
                    environment.ContentRootPath,
                    rutaConfigurada));

        Directory.CreateDirectory(_directorioRaiz);
    }

    public async Task<string> GuardarAsync(
        Stream contenido,
        string extension,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        if (string.IsNullOrWhiteSpace(extension) ||
            !extension.StartsWith('.') ||
            extension.Contains('/') ||
            extension.Contains('\\'))
        {
            throw new ArgumentException(
                "La extension del archivo no es valida.");
        }

        var nombreArchivo =
            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var rutaCompleta = ResolverRuta(nombreArchivo);

        await using var destino = new FileStream(
            rutaCompleta,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            options: FileOptions.Asynchronous);

        await contenido.CopyToAsync(
            destino,
            cancellationToken);

        return nombreArchivo;
    }

    public Task<Stream?> AbrirLecturaAsync(
        string rutaArchivo,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rutaCompleta = ResolverRuta(rutaArchivo);

        if (!File.Exists(rutaCompleta))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(
            rutaCompleta,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous |
                     FileOptions.SequentialScan);

        return Task.FromResult<Stream?>(stream);
    }

    public Task EliminarAsync(
        string rutaArchivo,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rutaCompleta = ResolverRuta(rutaArchivo);

        if (File.Exists(rutaCompleta))
            File.Delete(rutaCompleta);

        return Task.CompletedTask;
    }

    private string ResolverRuta(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo) ||
            !string.Equals(
                Path.GetFileName(rutaArchivo),
                rutaArchivo,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "La ruta del archivo no es valida.");
        }

        var rutaCompleta = Path.GetFullPath(
            Path.Combine(_directorioRaiz, rutaArchivo));

        if (!rutaCompleta.StartsWith(
                _directorioRaiz + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La ruta del archivo esta fuera del almacenamiento.");
        }

        return rutaCompleta;
    }
}