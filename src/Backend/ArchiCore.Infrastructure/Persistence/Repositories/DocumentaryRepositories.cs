using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Domain.Archives;
using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.Digitization;
using ArchiCore.Domain.Folders;
using ArchiCore.Domain.Loans;
using ArchiCore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence.Repositories;

internal sealed class ArchiveRepository(ArchiCoreDbContext dbContext) : IArchiveRepository
{
    public Task<Archive?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Archives.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Archive?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        => dbContext.Archives.SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);

    public async Task AddAsync(Archive archive, CancellationToken cancellationToken = default)
        => await dbContext.Archives.AddAsync(archive, cancellationToken);
}

internal sealed class FolderRepository(ArchiCoreDbContext dbContext) : IFolderRepository
{
    public Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Folders.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Folder?> GetByCodeAsync(FolderCode code, CancellationToken cancellationToken = default)
        => dbContext.Folders.SingleOrDefaultAsync(x => x.FolderCode == code, cancellationToken);

    public async Task<IReadOnlyList<Folder>> ListByArchiveAsync(Guid archiveId, int skip, int take, CancellationToken cancellationToken = default)
        => await dbContext.Folders.AsNoTracking().Where(x => x.ArchiveId == archiveId)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);

    public Task<int> CountByArchiveAsync(Guid archiveId, CancellationToken cancellationToken = default)
        => dbContext.Folders.CountAsync(x => x.ArchiveId == archiveId, cancellationToken);

    public async Task AddAsync(Folder folder, CancellationToken cancellationToken = default)
        => await dbContext.Folders.AddAsync(folder, cancellationToken);
}

internal sealed class BoxRepository(ArchiCoreDbContext dbContext) : IBoxRepository
{
    public Task<Box?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Boxes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Box?> GetByCodeAsync(BoxCode code, CancellationToken cancellationToken = default)
        => dbContext.Boxes.SingleOrDefaultAsync(x => x.BoxCode == code, cancellationToken);

    public async Task<IReadOnlyList<Box>> ListAsync(int skip, int take, CancellationToken cancellationToken = default)
        => await dbContext.Boxes.AsNoTracking().OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => dbContext.Boxes.CountAsync(cancellationToken);

    public async Task AddAsync(Box box, CancellationToken cancellationToken = default)
        => await dbContext.Boxes.AddAsync(box, cancellationToken);
}

internal sealed class LoanRepository(ArchiCoreDbContext dbContext) : ILoanRepository
{
    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Loans.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Loan?> GetActiveByFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
        => dbContext.Loans.SingleOrDefaultAsync(
            x => x.FolderId == folderId && x.Status != LoanStatus.Returned,
            cancellationToken);

    public async Task<IReadOnlyList<Loan>> ListByFolderAsync(Guid folderId, int skip, int take, CancellationToken cancellationToken = default)
        => await dbContext.Loans.AsNoTracking().Where(x => x.FolderId == folderId)
            .OrderByDescending(x => x.RequestedAtUtc).ThenBy(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Loan>> ListDueAsync(DateTimeOffset evaluatedAtUtc, int skip, int take, CancellationToken cancellationToken = default)
        => await dbContext.Loans.AsNoTracking()
            .Where(x => x.Status == LoanStatus.Delivered && x.DueAtUtc < evaluatedAtUtc)
            .OrderBy(x => x.DueAtUtc).ThenBy(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);

    public async Task AddAsync(Loan loan, CancellationToken cancellationToken = default)
        => await dbContext.Loans.AddAsync(loan, cancellationToken);
}

internal sealed class DigitizationRequestRepository(ArchiCoreDbContext dbContext) : IDigitizationRequestRepository
{
    public Task<DigitizationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.DigitizationRequests.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DigitizationRequest>> ListByFolderAsync(Guid folderId, int skip, int take, CancellationToken cancellationToken = default)
        => await dbContext.DigitizationRequests.AsNoTracking().Where(x => x.FolderId == folderId)
            .OrderByDescending(x => x.RequestedAtUtc).ThenBy(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);

    public async Task AddAsync(DigitizationRequest request, CancellationToken cancellationToken = default)
        => await dbContext.DigitizationRequests.AddAsync(request, cancellationToken);
}
