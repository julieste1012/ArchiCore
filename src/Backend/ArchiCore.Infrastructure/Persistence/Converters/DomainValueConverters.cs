using ArchiCore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ArchiCore.Infrastructure.Persistence.Converters;

/// <summary>
/// Convierte los Value Objects de código a su representación relacional primitiva.
/// La creación desde base de datos vuelve a validar el valor; registros corruptos fallan de forma explícita.
/// </summary>
internal static class DomainValueConverters
{
    public static readonly ValueConverter<Nit, string> NitConverter =
        new(value => value.Value, value => ToNit(value));

    public static readonly ValueConverter<ContractNumber, string> ContractNumberConverter =
        new(value => value.Value, value => ToContractNumber(value));

    public static readonly ValueConverter<ProjectCode, string> ProjectCodeConverter =
        new(value => value.Value, value => ToProjectCode(value));

    public static readonly ValueConverter<WarehouseCode, string> WarehouseCodeConverter =
        new(value => value.Value, value => ToWarehouseCode(value));

    public static readonly ValueConverter<BoxCode, string> BoxCodeConverter =
        new(value => value.Value, value => ToBoxCode(value));

    public static readonly ValueConverter<Barcode, string> BarcodeConverter =
        new(value => value.Value, value => ToBarcode(value));

    private static Nit ToNit(string value) => EnsureValid(ArchiCore.Domain.ValueObjects.Nit.Create(value), "NIT");
    private static ContractNumber ToContractNumber(string value) => EnsureValid(ArchiCore.Domain.ValueObjects.ContractNumber.Create(value), "ContractNumber");
    private static ProjectCode ToProjectCode(string value) => EnsureValid(ArchiCore.Domain.ValueObjects.ProjectCode.Create(value), "ProjectCode");
    private static WarehouseCode ToWarehouseCode(string value) => EnsureValid(ArchiCore.Domain.ValueObjects.WarehouseCode.Create(value), "WarehouseCode");
    private static BoxCode ToBoxCode(string value) => EnsureValid(ArchiCore.Domain.ValueObjects.BoxCode.Create(value), "BoxCode");
    private static Barcode ToBarcode(string value) => EnsureValid(ArchiCore.Domain.ValueObjects.Barcode.Create(value), "Barcode");

    private static TValue EnsureValid<TValue>(ArchiCore.Domain.Common.Result<TValue> result, string name)
        => result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"The persisted {name} value is invalid: {result.Error.Code}.");
}
