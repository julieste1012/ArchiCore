namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Clase base abstracta para Value Objects que representan un identificador
/// textual de negocio (códigos). Centraliza el almacenamiento del valor,
/// la igualdad estructural y su representación textual.
/// </summary>
public abstract class CodeValueObject : ValueObject
{
    /// <summary>
    /// Valor del código.
    /// </summary>
    public string Value { get; }

    protected CodeValueObject(string value)
    {
        Value = value;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
