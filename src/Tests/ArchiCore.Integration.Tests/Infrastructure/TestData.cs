namespace ArchiCore.Integration.Tests.Infrastructure;

/// <summary>
/// Generates identifiers that are unique per call and valid for the domain Value Objects
/// (prefixes CTR-, PRJ-, WH-; maximum length 20 characters).
/// </summary>
public static class TestData
{
    private static readonly int RunId = Random.Shared.Next(100_000, 1_000_000);
    private static long _sequence;

    private static long Next() => Interlocked.Increment(ref _sequence);

    public static string Nit() => $"{RunId}{Next():D6}-7";

    public static string ContractNumber() => $"CTR-{RunId}-{Next():D6}";

    public static string ProjectCode() => $"PRJ-{RunId}-{Next():D6}";

    public static string WarehouseCode() => $"WH-{RunId}-{Next():D6}";
}
