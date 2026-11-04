using ArchiCore.Domain.Folders;
using ArchiCore.Domain.Folders.Events;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Folders;

public sealed class FolderTests
{
    private static Folder CreateFolder()
        => Folder.Create(FolderCode.Create("FLD-000001").Value, Guid.NewGuid(), Guid.NewGuid()).Value;

    [Fact]
    public void Create_requires_archive_and_expedient_identity()
    {
        var result = Folder.Create(FolderCode.Create("FLD-000001").Value, Guid.Empty, Guid.NewGuid());
        Assert.True(result.IsFailure);
        Assert.Equal("Folder.ArchiveRequired", result.Error.Code);
    }

    [Fact]
    public void Folder_follows_documented_lifecycle_and_records_loan_identity()
    {
        var folder = CreateFolder();
        Assert.Equal(FolderStatus.Created, folder.Status);
        Assert.True(folder.Receive().IsSuccess);
        Assert.True(folder.Organize().IsSuccess);

        var boxId = Guid.NewGuid();
        Assert.True(folder.Store(boxId).IsSuccess);
        Assert.Equal(boxId, folder.BoxId);

        var loanId = Guid.NewGuid();
        Assert.True(folder.StartLoan(loanId).IsSuccess);
        Assert.Equal(loanId, folder.LoanId);
        Assert.True(folder.ReturnLoan().IsSuccess);
        Assert.Null(folder.LoanId);
        Assert.True(folder.Archive().IsSuccess);
        Assert.Equal(FolderStatus.Archived, folder.Status);
    }

    [Fact]
    public void Folder_can_move_to_another_box_without_changing_its_stored_state()
    {
        var folder = CreateFolder();
        Assert.True(folder.Receive().IsSuccess);
        Assert.True(folder.Organize().IsSuccess);
        var firstBox = Guid.NewGuid();
        var secondBox = Guid.NewGuid();
        Assert.True(folder.Store(firstBox).IsSuccess);

        var result = folder.MoveToBox(secondBox);

        Assert.True(result.IsSuccess);
        Assert.Equal(FolderStatus.Stored, folder.Status);
        Assert.Equal(secondBox, folder.BoxId);
        var moved = Assert.IsType<FolderMoved>(folder.DomainEvents.Last());
        Assert.Equal(firstBox, moved.PreviousBoxId);
        Assert.Equal(secondBox, moved.NewBoxId);
    }

    [Fact]
    public void Document_foliation_is_sequential_and_annexes_are_appended_after_folios()
    {
        var folder = CreateFolder();
        Assert.True(folder.Receive().IsSuccess);

        var documentId = folder.AddDocument("Acta de entrega").Value;
        Assert.Equal(1, folder.AddFolio(documentId, 1).Value);
        Assert.Equal(2, folder.AddFolio(documentId, 2).Value);

        var invalidFolio = folder.AddFolio(documentId, 4);
        Assert.True(invalidFolio.IsFailure);
        Assert.Equal("Folder.InvalidFolioSequence", invalidFolio.Error.Code);

        Assert.Equal(3, folder.AddAnnex(documentId, "Anexo fotográfico").Value);
        var document = Assert.Single(folder.Documents);
        Assert.Equal(new[] { 1, 2 }, document.Folios.Select(item => item.Number));
        Assert.Equal(3, Assert.Single(document.Annexes).Sequence);
        Assert.Contains(folder.DomainEvents, item => item is FolioAdded);
        Assert.Contains(folder.DomainEvents, item => item is AnnexAdded);
    }

    [Fact]
    public void Archived_folder_rejects_document_changes_and_lifecycle_mutations()
    {
        var folder = CreateFolder();
        Assert.True(folder.Receive().IsSuccess);
        Assert.True(folder.Organize().IsSuccess);
        Assert.True(folder.Store(Guid.NewGuid()).IsSuccess);
        Assert.True(folder.StartLoan(Guid.NewGuid()).IsSuccess);
        Assert.True(folder.ReturnLoan().IsSuccess);
        Assert.True(folder.Archive().IsSuccess);

        Assert.True(folder.AddDocument("No permitido").IsFailure);
        Assert.True(folder.AddFolio(Guid.NewGuid(), 1).IsFailure);
        Assert.True(folder.StartLoan(Guid.NewGuid()).IsFailure);
        Assert.Equal(FolderStatus.Archived, folder.Status);
    }
}
