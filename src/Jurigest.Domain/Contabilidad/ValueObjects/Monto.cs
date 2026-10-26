namespace Jurigest.Domain.Contabilidad.ValueObjects;

public sealed record Monto
{
    public Monto(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentOutOfRangeException(nameof(valor), "El monto no puede ser negativo.");

        Valor = valor;
    }

    public decimal Valor { get; }

    public static implicit operator decimal(Monto monto) => monto.Valor;

    public static implicit operator Monto(decimal valor) => new(valor);

    public override string ToString() => Valor.ToString("0.##");
}
