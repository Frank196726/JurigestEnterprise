namespace Jurigest.Domain.Judicial;

public sealed class ReciboEmitidoException() : InvalidOperationException(
    "La diligencia ya tiene un recibo emitido. No se puede cambiar su valor ni la gestión asociada desde esta pantalla.");
