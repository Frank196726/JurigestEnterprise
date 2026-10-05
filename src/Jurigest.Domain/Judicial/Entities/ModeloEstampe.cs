using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class ModeloEstampe : Entity<Guid>
{
    private ModeloEstampe() { }

    public ModeloEstampe(Guid id, string nombre, TipoDiligencia tipoDiligencia,
        string contenido, ResultadoDiligencia? resultado = null,
        Guid? diligenciaRealizadaId = null) : base(id)
    {
        CambiarNombre(nombre);
        CambiarTipoDiligencia(tipoDiligencia);
        CambiarResultado(resultado);
        CambiarDiligenciaRealizada(diligenciaRealizadaId);
        CambiarContenido(contenido);
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
        FechaActualizacion = FechaCreacion;
    }

    public string Nombre { get; private set; } = string.Empty;
    public TipoDiligencia TipoDiligencia { get; private set; }
    public ResultadoDiligencia? Resultado { get; private set; }
    public Guid? DiligenciaRealizadaId { get; private set; }
    public string Contenido { get; private set; } = string.Empty;
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaActualizacion { get; private set; }

    public void Actualizar(string nombre, TipoDiligencia tipoDiligencia,
        string contenido, ResultadoDiligencia? resultado, Guid? diligenciaRealizadaId)
    {
        CambiarNombre(nombre);
        CambiarTipoDiligencia(tipoDiligencia);
        CambiarResultado(resultado);
        CambiarDiligenciaRealizada(diligenciaRealizadaId);
        CambiarContenido(contenido);
        FechaActualizacion = DateTime.UtcNow;
    }

    public void Activar() { Activo = true; FechaActualizacion = DateTime.UtcNow; }
    public void Desactivar() { Activo = false; FechaActualizacion = DateTime.UtcNow; }

    private void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 200)
            throw new ArgumentException("El nombre del modelo es obligatorio y admite hasta 200 caracteres.", nameof(nombre));
        Nombre = nombre.Trim();
    }

    private void CambiarTipoDiligencia(TipoDiligencia tipoDiligencia)
    {
        if ((int)tipoDiligencia <= 0)
            throw new ArgumentException("El tipo de diligencia no es válido.", nameof(tipoDiligencia));
        TipoDiligencia = tipoDiligencia;
    }

    private void CambiarResultado(ResultadoDiligencia? resultado)
    {
        if (resultado.HasValue && (!Enum.IsDefined(resultado.Value) ||
            resultado.Value == ResultadoDiligencia.SinResultado))
            throw new ArgumentException("La clasificación del resultado no es válida.", nameof(resultado));
        Resultado = resultado;
    }

    private void CambiarDiligenciaRealizada(Guid? diligenciaRealizadaId)
    {
        if (diligenciaRealizadaId == Guid.Empty)
            throw new ArgumentException("La diligencia realizada no es válida.", nameof(diligenciaRealizadaId));
        DiligenciaRealizadaId = diligenciaRealizadaId;
    }

    private void CambiarContenido(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido) || contenido.Trim().Length > 8000)
            throw new ArgumentException("El contenido del modelo es obligatorio y admite hasta 8.000 caracteres.", nameof(contenido));
        Contenido = contenido.Trim();
    }
}
