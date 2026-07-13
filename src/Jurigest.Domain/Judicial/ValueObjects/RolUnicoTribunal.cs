namespace Jurigest.Domain.Judicial.ValueObjects;

/// <summary>
/// Representa el Rol Único de una causa judicial (RIT).
/// </summary>
public sealed class RolUnicoTribunal : IEquatable<RolUnicoTribunal>
{
    public RolUnicoTribunal(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("El RIT es obligatorio.", nameof(valor));

        Valor = valor.Trim().ToUpperInvariant();
    }

    public string Valor { get; }

    public override string ToString() => Valor;

    public bool Equals(RolUnicoTribunal? other)
        => other is not null && Valor == other.Valor;

    public override bool Equals(object? obj)
        => Equals(obj as RolUnicoTribunal);

    public override int GetHashCode()
        => Valor.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(RolUnicoTribunal? left, RolUnicoTribunal? right)
        => Equals(left, right);

    public static bool operator !=(RolUnicoTribunal? left, RolUnicoTribunal? right)
        => !(left == right);
}