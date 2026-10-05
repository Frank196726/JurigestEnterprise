namespace Jurigest.Domain.Judicial.Catalogos;
public sealed class MateriaCatalogo
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    private MateriaCatalogo() { }
    public MateriaCatalogo(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 200)
            throw new ArgumentException("Ingrese una materia de hasta 200 caracteres.");
        Id = Guid.NewGuid(); Nombre = nombre.Trim();
    }
}
