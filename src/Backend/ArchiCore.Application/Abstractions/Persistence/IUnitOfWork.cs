namespace ArchiCore.Application.Abstractions.Persistence;

/// <summary>
/// Abstracción de la unidad de trabajo para guardar de forma atómica los cambios del caso de uso.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
