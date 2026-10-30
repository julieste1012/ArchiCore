using ArchiCore.Domain.Boxes;

namespace ArchiCore.Application.Boxes;

public sealed record BoxDto(
    Guid Id,
    string BoxCode,
    string? Barcode,
    BoxStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateBoxRequest(string BoxCode);

public sealed record AssignBoxBarcodeRequest(string Barcode);
