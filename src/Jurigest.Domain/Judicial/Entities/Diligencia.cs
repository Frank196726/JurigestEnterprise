using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class Diligencia : Entity<Guid>
{
    private Diligencia()
    {
    }

    public Diligencia(
        Guid id,
        Guid causaId,
        string descripcion)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException(
                "La descripci�n es obligatoria.",
                nameof(descripcion));

        CausaId = causaId;
        Descripcion = descripcion.Trim();

        Tipo = TipoDiligencia.Otro;
        Estado = EstadoDiligencia.Pendiente;

        FechaCreacion = DateTime.UtcNow;
    }

    public Guid CausaId { get; private set; }

    public string Descripcion { get; private set; } = string.Empty;

    public TipoDiligencia Tipo { get; private set; }

    public EstadoDiligencia Estado { get; private set; }

    public ResultadoDiligencia Resultado { get; private set; }
            = ResultadoDiligencia.SinResultado;

    public string? ResultadoDetalle { get; private set; }

    public string? Estampe { get; private set; }

    public DateTime? FechaGestion { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public DateTime? FechaProgramada { get; private set; }

    public DateTime? FechaRealizada { get; private set; }

    public string? ReceptorJudicial { get; private set; }

    public string? Direccion { get; private set; }

    public string? Comuna { get; private set; }

    public string? Observaciones { get; private set; }

    public decimal? Latitud { get; private set; }

    public decimal? Longitud { get; private set; }

    public void ActualizarDatos(
        string descripcion,
        TipoDiligencia tipo,
        DateTime? fechaProgramada,
        string? receptorJudicial,
        string? direccion,
        string? comuna,
        string? observaciones)
    {
        if (Estado != EstadoDiligencia.Pendiente)
            throw new InvalidOperationException(
                "Solo se puede modificar una diligencia pendiente.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException(
                "La descripción es obligatoria.",
                nameof(descripcion));

        if (descripcion.Trim().Length > 500)
            throw new ArgumentException(
                "La descripción no puede superar 500 caracteres.",
                nameof(descripcion));

        var tieneDireccion = !string.IsNullOrWhiteSpace(direccion);
        var tieneComuna = !string.IsNullOrWhiteSpace(comuna);

        if (tieneDireccion != tieneComuna)
            throw new ArgumentException(
                "Debe indicar dirección y comuna, o dejar ambos campos vacíos.");

        Descripcion = descripcion.Trim();
        Tipo = tipo;
        FechaProgramada = fechaProgramada;
        ReceptorJudicial = string.IsNullOrWhiteSpace(receptorJudicial)
            ? null
            : receptorJudicial.Trim();
        Direccion = tieneDireccion ? direccion!.Trim() : null;
        Comuna = tieneComuna ? comuna!.Trim() : null;
        Observaciones = string.IsNullOrWhiteSpace(observaciones)
            ? null
            : observaciones.Trim();
    }

    public void Programar(DateTime fecha)
    {
        if (Estado != EstadoDiligencia.Pendiente &&
            Estado != EstadoDiligencia.EnProceso)
        {
            throw new InvalidOperationException(
                "Solo se puede programar una diligencia pendiente o en proceso.");
        }

        FechaProgramada = fecha;
    }

    public void CambiarTipo(TipoDiligencia tipo)
    {
        if (Estado == EstadoDiligencia.Completada ||
            Estado == EstadoDiligencia.Cancelada)
        {
            throw new InvalidOperationException(
                "No se puede cambiar el tipo de una diligencia finalizada o cancelada.");
        }

        Tipo = tipo;
    }

    public void AsignarReceptor(string receptor)
    {
        if (string.IsNullOrWhiteSpace(receptor))
            throw new ArgumentException(
                "El receptor judicial es obligatorio.",
                nameof(receptor));

        if (Estado == EstadoDiligencia.Completada ||
            Estado == EstadoDiligencia.Cancelada)
        {
            throw new InvalidOperationException(
                "No se puede cambiar el receptor de una diligencia finalizada o cancelada.");
        }

        ReceptorJudicial = receptor.Trim();
    }

    public void AsignarUbicacion(
        string direccion,
        string comuna)
    {
        if (string.IsNullOrWhiteSpace(direccion))
            throw new ArgumentException(
                "La direcci�n es obligatoria.",
                nameof(direccion));

        if (string.IsNullOrWhiteSpace(comuna))
            throw new ArgumentException(
                "La comuna es obligatoria.",
                nameof(comuna));

        Direccion = direccion.Trim();
        Comuna = comuna.Trim();
    }

    public void RegistrarCoordenadas(
        decimal latitud,
        decimal longitud)
    {
        if (latitud < -90 || latitud > 90)
            throw new ArgumentOutOfRangeException(
                nameof(latitud),
                "La latitud debe estar entre -90 y 90.");

        if (longitud < -180 || longitud > 180)
            throw new ArgumentOutOfRangeException(
                nameof(longitud),
                "La longitud debe estar entre -180 y 180.");

        Latitud = latitud;
        Longitud = longitud;
    }

    public void AgregarObservacion(string observacion)
    {
        if (string.IsNullOrWhiteSpace(observacion))
            throw new ArgumentException(
                "La observaci�n es obligatoria.",
                nameof(observacion));

        Observaciones = observacion.Trim();
    }

    public void Completar()
    {
        if (Estado != EstadoDiligencia.EnProceso)
        {
            throw new InvalidOperationException(
                "Solo se puede completar una diligencia que est� en proceso.");
        }

        Estado = EstadoDiligencia.Completada;
        FechaRealizada = DateTime.UtcNow;
    }

    public void Iniciar()
    {
        if (Estado != EstadoDiligencia.Pendiente)
        {
            throw new InvalidOperationException(
                "Solo se puede iniciar una diligencia pendiente.");
        }

        Estado = EstadoDiligencia.EnProceso;
    }

    public void Suspender()
    {
        if (Estado != EstadoDiligencia.EnProceso)
        {
            throw new InvalidOperationException(
                "Solo se puede suspender una diligencia que est� en proceso.");
        }

        Estado = EstadoDiligencia.Suspendida;
    }

    public void Rechazar()
    {
        if (Estado == EstadoDiligencia.Completada ||
            Estado == EstadoDiligencia.Cancelada)
        {
            throw new InvalidOperationException(
                "No se puede rechazar una diligencia finalizada o cancelada.");
        }

        Estado = EstadoDiligencia.Rechazada;
    }

    public void RegistrarResultado(
        ResultadoDiligencia resultado,
        string resultadoDetalle,
        string estampe,
        DateTime fechaGestion)
    {
        if (resultado == ResultadoDiligencia.SinResultado)
        {
            throw new ArgumentException(
                "Debe indicar la clasificación del resultado de la diligencia.",
                nameof(resultado));
        }

        if (string.IsNullOrWhiteSpace(resultadoDetalle))
        {
            throw new ArgumentException(
                "Debe indicar el resultado de la diligencia.",
                nameof(resultadoDetalle));
        }

        if (resultadoDetalle.Trim().Length > 500)
        {
            throw new ArgumentException(
                "El resultado de la diligencia no puede superar 500 caracteres.",
                nameof(resultadoDetalle));
        }

        if (string.IsNullOrWhiteSpace(estampe))
        {
            throw new ArgumentException(
                "El estampe de la diligencia es obligatorio.",
                nameof(estampe));
        }

        if (fechaGestion == default)
        {
            throw new ArgumentException(
                "La fecha de gestión es obligatoria.",
                nameof(fechaGestion));
        }

        if (Estado == EstadoDiligencia.Cancelada)
        {
            throw new InvalidOperationException(
                "No se puede registrar resultado en una diligencia cancelada.");
        }

        Resultado = resultado;

        ResultadoDetalle = resultadoDetalle.Trim();

        Estampe = estampe.Trim();

        FechaGestion = fechaGestion;

        Estado = EstadoDiligencia.Completada;
    }
}
