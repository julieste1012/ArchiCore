using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.Boxes.Events;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Boxes;

public sealed class BoxTests
{
    private static Box CreateBox()
        => Box.Create(BoxCode.Create("BOX-000001").Value).Value;

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
    public void Assigning_a_barcode_emits_barcode_assigned()
    {
        var box = CreateBox();

        var result = box.AssignBarcode(Barcode.Create("BC-000001").Value);

        Assert.True(result.IsSuccess);
        Assert.Equal("BC-000001", box.Barcode!.Value);
        Assert.IsType<BarcodeAssigned>(box.DomainEvents.Last());
    }

    [Fact]
    public void Moving_a_box_from_created_state_is_rejected()
    {
        var box = CreateBox();

        var result = box.Move();

        Assert.True(result.IsFailure);
        Assert.Equal("Box.InvalidMove", result.Error.Code);
        Assert.Equal(BoxStatus.Created, box.Status);
    }
}
