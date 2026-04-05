using GuedesPlace.AzureTools.Functions.FunctionRunContext;

namespace GuedesPlace.AzureTools.Tests.Functions;

/// <summary>
/// Tests for the shared role-checking logic in FunctionRunContext base class,
/// exercised through AppFunctionRunContext (simplest concrete subclass).
/// </summary>
public class FunctionRunContextRolesTests
{
    [Fact]
    public void IsInAtLeastOneRole_WithMatchingRole_ReturnsTrue()
    {
        var ctx = new AppFunctionRunContext("app1", ["admin", "reader"]);
        Assert.True(ctx.IsInAtLeastOneRole("admin"));
    }

    [Fact]
    public void IsInAtLeastOneRole_WithOneOfMultipleMatching_ReturnsTrue()
    {
        var ctx = new AppFunctionRunContext("app1", ["reader"]);
        Assert.True(ctx.IsInAtLeastOneRole("admin", "reader", "writer"));
    }

    [Fact]
    public void IsInAtLeastOneRole_WithNoMatchingRole_ReturnsFalse()
    {
        var ctx = new AppFunctionRunContext("app1", ["reader"]);
        Assert.False(ctx.IsInAtLeastOneRole("admin", "writer"));
    }

    [Fact]
    public void IsInAtLeastOneRole_WithEmptyRolesToCheck_ReturnsFalse()
    {
        var ctx = new AppFunctionRunContext("app1", ["admin"]);
        Assert.False(ctx.IsInAtLeastOneRole());
    }

    [Fact]
    public void IsInAtLeastOneRole_WithEmptyAssignedRoles_ReturnsFalse()
    {
        var ctx = new AppFunctionRunContext("app1", []);
        Assert.False(ctx.IsInAtLeastOneRole("admin"));
    }

    [Fact]
    public void IsInAllRoles_WithAllRolesPresent_ReturnsTrue()
    {
        var ctx = new AppFunctionRunContext("app1", ["admin", "reader", "writer"]);
        Assert.True(ctx.IsInAllRoles("admin", "reader"));
    }

    [Fact]
    public void IsInAllRoles_WithExactMatch_ReturnsTrue()
    {
        var ctx = new AppFunctionRunContext("app1", ["admin"]);
        Assert.True(ctx.IsInAllRoles("admin"));
    }

    [Fact]
    public void IsInAllRoles_WithOneMissing_ReturnsFalse()
    {
        var ctx = new AppFunctionRunContext("app1", ["admin", "reader"]);
        Assert.False(ctx.IsInAllRoles("admin", "reader", "writer"));
    }

    [Fact]
    public void IsInAllRoles_WithNoRolesAssigned_ReturnsFalse()
    {
        var ctx = new AppFunctionRunContext("app1", []);
        Assert.False(ctx.IsInAllRoles("admin"));
    }

    [Fact]
    public void IsInAllRoles_WithEmptyRolesToCheck_ReturnsTrue()
    {
        // Intersection of empty set with any set is empty; empty == empty (count 0 == 0)
        var ctx = new AppFunctionRunContext("app1", ["admin"]);
        Assert.True(ctx.IsInAllRoles());
    }
}
