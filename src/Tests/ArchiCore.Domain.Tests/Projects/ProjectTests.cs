using ArchiCore.Domain.Projects;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Projects;

public sealed class ProjectTests
{
    private static Project CreateProject()
        => Project.Create(ProjectCode.Create("PRJ-2026-000001").Value, Guid.NewGuid()).Value;

    [Fact]
    public void Create_requires_a_contract_id()
    {
        var result = Project.Create(ProjectCode.Create("PRJ-2026-000001").Value, Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal("Project.ContractIdRequired", result.Error.Code);
    }

    [Fact]
    public void Project_lifecycle_follows_the_implemented_transitions()
    {
        var project = CreateProject();

        Assert.Equal(ProjectStatus.Draft, project.Status);
        Assert.True(project.Plan().IsSuccess);
        Assert.Equal(ProjectStatus.Planned, project.Status);

        Assert.True(project.Start().IsSuccess);
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.NotNull(project.StartDate);

        Assert.True(project.Suspend().IsSuccess);
        Assert.True(project.Resume().IsSuccess);
        Assert.True(project.Complete().IsSuccess);
        Assert.Equal(ProjectStatus.Completed, project.Status);
        Assert.NotNull(project.EndDate);

        Assert.True(project.Close().IsSuccess);
        Assert.True(project.Archive().IsSuccess);
        Assert.Equal(ProjectStatus.Archived, project.Status);
    }

    [Fact]
    public void Project_cannot_start_before_being_planned()
    {
        var project = CreateProject();

        var result = project.Start();

        Assert.True(result.IsFailure);
        Assert.Equal("Project.InvalidStart", result.Error.Code);
        Assert.Equal(ProjectStatus.Draft, project.Status);
    }

    [Fact]
    public void Project_cannot_have_two_active_assignments_for_the_same_warehouse()
    {
        var project = CreateProject();
        var warehouseId = Guid.NewGuid();
        var assignedFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.True(project.AssignWarehouse(warehouseId, assignedFrom).IsSuccess);

        var duplicate = project.AssignWarehouse(warehouseId, assignedFrom.AddDays(1));

        Assert.True(duplicate.IsFailure);
        Assert.Equal("Project.DuplicateWarehouseAssignment", duplicate.Error.Code);
        Assert.Single(project.WarehouseAssignments);
    }

    [Fact]
    public void Released_warehouse_assignment_can_be_reassigned()
    {
        var project = CreateProject();
        var warehouseId = Guid.NewGuid();
        var assignedFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var releasedOn = assignedFrom.AddDays(5);

        Assert.True(project.AssignWarehouse(warehouseId, assignedFrom).IsSuccess);
        Assert.True(project.ReleaseWarehouse(warehouseId, releasedOn).IsSuccess);
        Assert.True(project.AssignWarehouse(warehouseId, releasedOn.AddDays(1)).IsSuccess);

        Assert.Equal(2, project.WarehouseAssignments.Count);
        Assert.Equal(1, project.WarehouseAssignments.Count(x => x.IsActive));
    }

    [Fact]
    public void Assignment_cannot_be_released_before_it_starts()
    {
        var project = CreateProject();
        var warehouseId = Guid.NewGuid();
        var assignedFrom = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        Assert.True(project.AssignWarehouse(warehouseId, assignedFrom).IsSuccess);

        var result = project.ReleaseWarehouse(warehouseId, assignedFrom.AddDays(-1));

        Assert.True(result.IsFailure);
        Assert.Equal("Project.InvalidReleaseDate", result.Error.Code);
        Assert.True(project.WarehouseAssignments.Single().IsActive);
    }

    [Fact]
    public void Empty_coordinator_id_is_rejected()
    {
        var project = CreateProject();

        var result = project.AssignCoordinator(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal("Project.CoordinatorIdRequired", result.Error.Code);
    }
}
