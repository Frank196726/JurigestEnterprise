namespace Jurigest.Web.Security;

public interface ISesionWebStore
{
    string Crear(SesionWeb sesion);

    SesionWeb? Obtener(string identificador);

    void Actualizar(
        string identificador,
        SesionWeb sesion);

    void Eliminar(string identificador);
}