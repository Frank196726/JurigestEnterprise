namespace Jurigest.API;

public static class ExcepcionesExtensions
{
    /// <summary>
    /// Mensaje para mostrar al usuario. ArgumentException agrega " (Parameter 'x')" cuando
    /// se indica el nombre del parámetro; ese detalle técnico no debe llegar a la pantalla.
    /// </summary>
    public static string MensajeParaUsuario(this Exception ex) =>
        ex is ArgumentException { ParamName: { } parametro } && ex.Message.EndsWith($" (Parameter '{parametro}')")
            ? ex.Message[..^$" (Parameter '{parametro}')".Length]
            : ex.Message;
}
