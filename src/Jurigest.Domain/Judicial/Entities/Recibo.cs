using Jurigest.Domain.Judicial.Enums;
using Jurigest.Domain.Kernel.Common;

namespace Jurigest.Domain.Judicial.Entities;

public sealed class Recibo : Entity<Guid>
{
    private Recibo()
    {
    }

    public Recibo(
        Guid id,
        Guid causaId,
        Guid diligenciaId,
        Guid diligenciaRealizadaId,
        string diligenciaRealizada,
        decimal monto,
        DateTime fechaEmision)
        : base(id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del recibo es obligatorio.",
                nameof(id));
        }

        if (causaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La causa es obligatoria.",
                nameof(causaId));
        }

        if (diligenciaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La diligencia es obligatoria.",
                nameof(diligenciaId));
        }

        if (diligenciaRealizadaId == Guid.Empty)
        {
            throw new ArgumentException(
                "La diligencia realizada es obligatoria.",
                nameof(diligenciaRealizadaId));
        }

        if (string.IsNullOrWhiteSpace(diligenciaRealizada))
        {
            throw new ArgumentException(
                "El nombre de la diligencia realizada es obligatorio.",
                nameof(diligenciaRealizada));
        }

        if (diligenciaRealizada.Trim().Length > 200)
        {
            throw new ArgumentException(
                "El nombre de la diligencia realizada no puede superar 200 caracteres.",
                nameof(diligenciaRealizada));
        }

        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(monto),
                "El monto del recibo debe ser mayor que cero.");
        }

        if (fechaEmision == default)
        {
            throw new ArgumentException(
                "La fecha de emisión es obligatoria.",
                nameof(fechaEmision));
        }

        CausaId = causaId;
        DiligenciaId = diligenciaId;
        DiligenciaRealizadaId = diligenciaRealizadaId;
        DiligenciaRealizada = diligenciaRealizada.Trim();
        Monto = monto;
        Estado = EstadoRecibo.Pendiente;
        FechaEmision = fechaEmision;
    }

    public Guid CausaId { get; private set; }

    public Guid DiligenciaId { get; private set; }

    public Guid DiligenciaRealizadaId { get; private set; }

    public string DiligenciaRealizada { get; private set; } = string.Empty;

    public decimal Monto { get; private set; }

    public EstadoRecibo Estado { get; private set; }

    public DateTime FechaEmision { get; private set; }

    public DateTime? FechaPago { get; private set; }

    public void MarcarPagado(DateTime fechaPago)
    {
        if (fechaPago == default)
        {
            throw new ArgumentException(
                "La fecha de pago es obligatoria.",
                nameof(fechaPago));
        }

        if (fechaPago < FechaEmision)
        {
            throw new InvalidOperationException(
                "La fecha de pago no puede ser anterior a la fecha de emisión.");
        }

        if (Estado == EstadoRecibo.Pagado)
        {
            return;
        }

        Estado = EstadoRecibo.Pagado;
        FechaPago = fechaPago;
    }
}
