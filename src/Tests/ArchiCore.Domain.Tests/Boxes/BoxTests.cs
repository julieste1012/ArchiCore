using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.Boxes.Events;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Boxes;

public sealed class BoxTests
{
    private static Box CreateBox()
        => Box.Create(BoxCode.Create("BOX-000001").Value).Value;

    private static Box CreateStoredBox()
    {
        var box = CreateBox();
        Assert.True(box.AssignBarcode(Barcode.Create("BC-000001").Value).IsSuccess);
        Assert.True(box.MakeAvailable().IsSuccess);
        Assert.True(box.Store(Guid.NewGuid()).IsSuccess);
        box.ClearDomainEvents();
        return box;
    }

    [Fact]
    public void Create_requires_a_box_code()
    {
        var result = Box.Create(null!);
        Assert.True(result.IsFailure);
        Assert.Equal("Box.BoxCodeRequired", result.Error.Code);
    }

    [Fact]
    public void Create_starts_as_created_and_emits_box_created()
    {
        var box = CreateBox();
        Assert.Equal(BoxStatus.Created, box.Status);
        Assert.IsType<BoxCreated>(Assert.Single(box.DomainEvents));
    }

    [Fact]
    public void Barcode_can_only_be_assigned_once_before_the_box_becomes_available()
    {
        var box = CreateBox();
        var barcode = Barcode.Create("BC-000001").Value;

        Assert.True(box.AssignBarcode(barcode).IsSuccess);
        var duplicate = box.AssignBarcode(Barcode.Create("BC-000002").Value);
        Assert.True(duplicate.IsFailure);
        Assert.Equal("Box.BarcodeAlreadyAssigned", duplicate.Error.Code);
        Assert.True(box.MakeAvailable().IsSuccess);
        var lateAssignment = box.AssignBarcode(Barcode.Create("BC-000003").Value);
        Assert.True(lateAssignment.IsFailure);
        Assert.Equal("Box.InvalidBarcodeAssignmentState", lateAssignment.Error.Code);
    }

    [Fact]
    public void Box_requires_barcode_before_becoming_available()
    {
        var box = CreateBox();
        var result = box.MakeAvailable();

        Assert.True(result.IsFailure);
        Assert.Equal("Box.BarcodeRequired", result.Error.Code);
        Assert.Equal(BoxStatus.Created, box.Status);
    }

    [Fact]
    public void Box_can_follow_the_documented_lifecycle()
    {
        var box = CreateBox();

        Assert.True(box.AssignBarcode(Barcode.Create("BC-000001").Value).IsSuccess);
        Assert.True(box.MakeAvailable().IsSuccess);
        Assert.Equal(BoxStatus.Available, box.Status);

        var locationId = Guid.NewGuid();
        Assert.True(box.Store(locationId).IsSuccess);
        Assert.Equal(BoxStatus.InStorage, box.Status);
        Assert.Equal(locationId, box.WarehouseLocationId);

        Assert.True(box.Move().IsSuccess);
        Assert.Equal(BoxStatus.InTransit, box.Status);
        Assert.Null(box.WarehouseLocationId);

        Assert.True(box.StartLoanProcess().IsSuccess);
        Assert.Equal(BoxStatus.LoanProcess, box.Status);

        Assert.True(box.Return().IsSuccess);
        Assert.True(box.Archive().IsSuccess);
        Assert.Equal(BoxStatus.Archived, box.Status);
    }

    [Fact]
    public void Move_from_created_state_is_rejected()
    {
        var box = CreateBox();
        var result = box.Move();

        Assert.True(result.IsFailure);
        Assert.Equal("Box.InvalidMove", result.Error.Code);
        Assert.Equal(BoxStatus.Created, box.Status);
    }

    [Fact]
    public void Folders_can_only_be_associated_once_and_removed_before_archival()
    {
        var box = CreateStoredBox();
        var folderId = Guid.NewGuid();

        Assert.True(box.AddFolder(folderId).IsSuccess);
        var duplicate = box.AddFolder(folderId);
        Assert.True(duplicate.IsFailure);
        Assert.Equal("Box.FolderAlreadyAssigned", duplicate.Error.Code);
        Assert.Single(box.FolderIds);

        Assert.True(box.RemoveFolder(folderId).IsSuccess);
        Assert.Empty(box.FolderIds);
        Assert.Contains(box.DomainEvents, item => item is FolderAdded);
        Assert.Contains(box.DomainEvents, item => item is FolderRemoved);
    }

    [Fact]
    public void A_box_with_assigned_folders_cannot_be_archived()
    {
        var box = CreateStoredBox();
        Assert.True(box.AddFolder(Guid.NewGuid()).IsSuccess);
        Assert.True(box.Move().IsSuccess);
        Assert.True(box.StartLoanProcess().IsSuccess);
        Assert.True(box.Return().IsSuccess);

        var result = box.Archive();

        Assert.True(result.IsFailure);
        Assert.Equal("Box.FoldersRemainAssigned", result.Error.Code);
        Assert.Equal(BoxStatus.Returned, box.Status);
    }
}
