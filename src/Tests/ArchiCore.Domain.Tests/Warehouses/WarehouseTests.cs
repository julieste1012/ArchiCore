using ArchiCore.Domain.Warehouses;
using ArchiCore.Domain.Warehouses.Events;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Warehouses;

public sealed class WarehouseTests
{
    private static Warehouse CreateWarehouse()
        => Warehouse.Create(WarehouseCode.Create("WH-001").Value).Value;

    [Fact]
    public void Create_starts_as_available_and_emits_warehouse_created()
    {
        var warehouse = CreateWarehouse();

        Assert.Equal(WarehouseStatus.Available, warehouse.Status);
        Assert.IsType<WarehouseCreated>(Assert.Single(warehouse.DomainEvents));
    }

    [Fact]
    public void Warehouse_can_activate_enter_maintenance_return_to_operation_close_and_archive()
    {
        var warehouse = CreateWarehouse();

        Assert.True(warehouse.Activate().IsSuccess);
        Assert.Equal(WarehouseStatus.Operational, warehouse.Status);

        Assert.True(warehouse.StartMaintenance().IsSuccess);
        Assert.Equal(WarehouseStatus.Maintenance, warehouse.Status);

        Assert.True(warehouse.FinishMaintenance().IsSuccess);
        Assert.Equal(WarehouseStatus.Operational, warehouse.Status);

        Assert.True(warehouse.StartMaintenance().IsSuccess);
        Assert.True(warehouse.Close().IsSuccess);
        Assert.Equal(WarehouseStatus.Closed, warehouse.Status);

        Assert.True(warehouse.Archive().IsSuccess);
        Assert.Equal(WarehouseStatus.Archived, warehouse.Status);
    }

    [Fact]
    public void Warehouse_cannot_be_activated_twice()
    {
        var warehouse = CreateWarehouse();

        Assert.True(warehouse.Activate().IsSuccess);
        var secondActivation = warehouse.Activate();

        Assert.True(secondActivation.IsFailure);
        Assert.Equal("Warehouse.InvalidActivation", secondActivation.Error.Code);
        Assert.Equal(WarehouseStatus.Operational, warehouse.Status);
    }

    [Fact]
    public void Warehouse_cannot_close_while_operational()
    {
        var warehouse = CreateWarehouse();
        Assert.True(warehouse.Activate().IsSuccess);

        var result = warehouse.Close();

        Assert.True(result.IsFailure);
        Assert.Equal("Warehouse.InvalidClose", result.Error.Code);
        Assert.Equal(WarehouseStatus.Operational, warehouse.Status);
    }
}
