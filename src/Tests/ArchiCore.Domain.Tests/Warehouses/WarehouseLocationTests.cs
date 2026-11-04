using ArchiCore.Domain.Warehouses;
using ArchiCore.Domain.Warehouses.Events;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Warehouses;

public sealed class WarehouseLocationTests
{
    private static Warehouse CreateOperationalWarehouse()
    {
        var warehouse = Warehouse.Create(WarehouseCode.Create("WH-001").Value).Value;
        Assert.True(warehouse.Activate().IsSuccess);
        warehouse.ClearDomainEvents();
        return warehouse;
    }

    private static Guid AddPosition(Warehouse warehouse, string code, int capacity = 1)
    {
        var prefix = code.Replace("-", string.Empty, StringComparison.Ordinal);
        var aisle = warehouse.AddLocation($"A-{prefix}", WarehouseLocationType.Aisle);
        Assert.True(aisle.IsSuccess);
        var shelf = warehouse.AddLocation($"E-{prefix}", WarehouseLocationType.Shelf, aisle.Value);
        Assert.True(shelf.IsSuccess);
        var cabinet = warehouse.AddLocation($"G-{prefix}", WarehouseLocationType.Cabinet, shelf.Value);
        Assert.True(cabinet.IsSuccess);
        var position = warehouse.AddLocation(code, WarehouseLocationType.Position, cabinet.Value, capacity);
        Assert.True(position.IsSuccess);
        return position.Value;
    }

    [Fact]
    public void AddLocation_requires_correct_parent_hierarchy()
    {
        var warehouse = CreateOperationalWarehouse();
        var result = warehouse.AddLocation("E-01", WarehouseLocationType.Shelf);
        Assert.True(result.IsFailure);
        Assert.Equal("Warehouse.LocationParentRequired", result.Error.Code);
    }

    [Fact]
    public void Position_requires_positive_capacity_and_emits_event()
    {
        var warehouse = CreateOperationalWarehouse();
        var aisle = warehouse.AddLocation("A-01", WarehouseLocationType.Aisle).Value;
        var shelf = warehouse.AddLocation("E-01", WarehouseLocationType.Shelf, aisle).Value;
        var cabinet = warehouse.AddLocation("G-01", WarehouseLocationType.Cabinet, shelf).Value;
        var invalid = warehouse.AddLocation("P-01", WarehouseLocationType.Position, cabinet, 0);
        Assert.True(invalid.IsFailure);
        Assert.Equal("Warehouse.InvalidLocationCapacity", invalid.Error.Code);
        var position = warehouse.AddLocation("P-01", WarehouseLocationType.Position, cabinet, 2);
        Assert.True(position.IsSuccess);
        Assert.Contains(warehouse.DomainEvents, item => item is LocationCreated);
    }

    [Fact]
    public void Position_capacity_is_enforced_and_released()
    {
        var warehouse = CreateOperationalWarehouse();
        var position = AddPosition(warehouse, "P-01");
        var firstBox = Guid.NewGuid();
        var secondBox = Guid.NewGuid();
        Assert.True(warehouse.OccupyLocation(position, firstBox).IsSuccess);
        var secondResult = warehouse.OccupyLocation(position, secondBox);
        Assert.True(secondResult.IsFailure);
        Assert.Equal("Warehouse.LocationCapacityExceeded", secondResult.Error.Code);
        Assert.True(warehouse.ReleaseLocation(position, firstBox).IsSuccess);
        Assert.True(warehouse.OccupyLocation(position, secondBox).IsSuccess);
        Assert.Equal(1, warehouse.GetLocationOccupancy(position).Value);
    }

    [Fact]
    public void A_box_cannot_occupy_two_locations_in_the_same_warehouse()
    {
        var warehouse = CreateOperationalWarehouse();
        var first = AddPosition(warehouse, "P-01", 2);
        var second = AddPosition(warehouse, "P-02", 2);
        var boxId = Guid.NewGuid();
        Assert.True(warehouse.OccupyLocation(first, boxId).IsSuccess);
        var result = warehouse.OccupyLocation(second, boxId);
        Assert.True(result.IsFailure);
        Assert.Equal("Warehouse.BoxAlreadyStored", result.Error.Code);
    }

    [Fact]
    public void MoveBox_updates_source_and_destination_and_emits_events()
    {
        var warehouse = CreateOperationalWarehouse();
        var source = AddPosition(warehouse, "P-01");
        var destination = AddPosition(warehouse, "P-02");
        var boxId = Guid.NewGuid();
        Assert.True(warehouse.OccupyLocation(source, boxId).IsSuccess);
        warehouse.ClearDomainEvents();

        var result = warehouse.MoveBox(boxId, destination);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, warehouse.GetLocationOccupancy(source).Value);
        Assert.Equal(1, warehouse.GetLocationOccupancy(destination).Value);
        Assert.Contains(warehouse.DomainEvents, item => item is LocationReleased);
        Assert.Contains(warehouse.DomainEvents, item => item is LocationOccupied);
        Assert.Contains(warehouse.DomainEvents, item => item is BoxRelocated);
    }

    [Fact]
    public void Warehouse_cannot_close_while_boxes_remain_stored()
    {
        var warehouse = CreateOperationalWarehouse();
        var position = AddPosition(warehouse, "P-01");
        Assert.True(warehouse.OccupyLocation(position, Guid.NewGuid()).IsSuccess);
        Assert.True(warehouse.StartMaintenance().IsSuccess);
        var result = warehouse.Close();
        Assert.True(result.IsFailure);
        Assert.Equal("Warehouse.OccupiedLocations", result.Error.Code);
        Assert.Equal(WarehouseStatus.Maintenance, warehouse.Status);
    }
}
