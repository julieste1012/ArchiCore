using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Contracts;

/// <summary>
/// Referencia por identidad de un proyecto asociado al contrato, utilizada
/// para preservar la invariante de unicidad de <see cref="ValueObjects.ProjectCode"/>
/// sin acoplar el Aggregate <see cref="Contract"/> al Aggregate Project.
/// </summary>
public sealed record ContractProjectReference(Guid ProjectId, ProjectCode ProjectCode);
