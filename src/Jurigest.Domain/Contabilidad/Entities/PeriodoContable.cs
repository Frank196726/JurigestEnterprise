using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Contabilidad.Entities;

public class PeriodoContable : Entity<Guid>
{
    protected PeriodoContable()
    {
    }

    public PeriodoContable(Guid id, DateOnly fechaInicio, DateOnly fechaFin, string descripcion)
        : base(id)
    {
        if (fechaFin < fechaInicio)
            throw new ArgumentException("La fecha final no puede ser anterior a la de inicio.", nameof(fechaFin));

        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Descripcion = descripcion.Trim();
        Cerrado = false;
    }

    public DateOnly FechaInicio { get; private set; }

    public DateOnly FechaFin { get; private set; }

    public string Descripcion { get; private set; } = string.Empty;

    public bool Cerrado { get; private set; }

    public void Cerrar()
    {
        Cerrado = true;
    }
}
