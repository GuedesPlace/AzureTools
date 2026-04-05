using Azure;
using Azure.Data.Tables;
using GuedesPlace.AzureTools.Tables;

namespace GuedesPlace.AzureTools.Tests.Tables;

public class ObjectBuilderTests
{
    private class SimplePoco
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
    }

    private class PocoWithDate
    {
        public DateTime? CreatedAt { get; set; }
    }

    private class NestedPoco
    {
        public string? Title { get; set; }
        public Address? Address { get; set; }
    }

    private class Address
    {
        public string? City { get; set; }
        public string? Street { get; set; }
    }

    private class PocoWithBytes
    {
        public byte[]? Data { get; set; }
    }

    private static TableEntity BuildTableEntity(Dictionary<string, object?> values)
    {
        var entity = new TableEntity("pk", "rk");
        foreach (var kv in values)
        {
            entity[kv.Key] = kv.Value;
        }
        return entity;
    }

    [Fact]
    public void Build_WithSimpleProperties_ReturnsCorrectValues()
    {
        var entity = BuildTableEntity(new()
        {
            ["Name"] = "Alice",
            ["Age"] = 30,
            ["Active"] = true
        });

        var result = ObjectBuilder.Build<SimplePoco>(entity);

        Assert.Equal("Alice", result.Name);
        Assert.Equal(30, result.Age);
        Assert.True(result.Active);
    }

    [Fact]
    public void Build_WithMissingKey_LeavesPropertyAtDefault()
    {
        var entity = BuildTableEntity(new()
        {
            ["Name"] = "Bob"
            // Age and Active are missing
        });

        var result = ObjectBuilder.Build<SimplePoco>(entity);

        Assert.Equal("Bob", result.Name);
        Assert.Equal(0, result.Age);
        Assert.False(result.Active);
    }

    [Fact]
    public void Build_WithDateTimeOffset_UsesWallClockTimeMarkedAsUtc()
    {
        // ObjectBuilder converts DateTimeOffset → DateTime via:
        //   DateTime.SpecifyKind(dtoValue.DateTime, DateTimeKind.Utc)
        // .DateTime keeps the wall-clock components without offset conversion,
        // so a value of 12:00+02:00 becomes 12:00Z, NOT the true UTC 10:00Z.
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.FromHours(2));
        var entity = BuildTableEntity(new()
        {
            ["CreatedAt"] = dto
        });

        var result = ObjectBuilder.Build<PocoWithDate>(entity);

        Assert.NotNull(result.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, result.CreatedAt!.Value.Kind);
        // Wall-clock time (12:00) is preserved, not converted to true UTC (10:00)
        Assert.Equal(new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc), result.CreatedAt.Value);
    }

    [Fact]
    public void Build_WithNestedObject_ReconstructsChildObject()
    {
        var entity = BuildTableEntity(new()
        {
            ["Title"] = "Report",
            ["Address_City"] = "Berlin",
            ["Address_Street"] = "Hauptstrasse"
        });

        var result = ObjectBuilder.Build<NestedPoco>(entity);

        Assert.Equal("Report", result.Title);
        Assert.NotNull(result.Address);
        Assert.Equal("Berlin", result.Address!.City);
        Assert.Equal("Hauptstrasse", result.Address.Street);
    }

    [Fact]
    public void Build_WithByteArray_PassesValueThrough()
    {
        var data = new byte[] { 10, 20, 30 };
        var entity = BuildTableEntity(new()
        {
            ["Data"] = data
        });

        var result = ObjectBuilder.Build<PocoWithBytes>(entity);

        Assert.Equal(data, result.Data);
    }

    [Fact]
    public void RoundTrip_SimplePoco_PreservesAllValues()
    {
        var original = new SimplePoco { Name = "Carol", Age = 25, Active = true };
        var serialized = ObjectSerializer.Serialize(original);

        var entity = new TableEntity("pk", "rk");
        foreach (var kv in serialized)
        {
            entity[kv.Key] = kv.Value;
        }

        var rebuilt = ObjectBuilder.Build<SimplePoco>(entity);

        Assert.Equal(original.Name, rebuilt.Name);
        Assert.Equal(original.Age, rebuilt.Age);
        Assert.Equal(original.Active, rebuilt.Active);
    }

    [Fact]
    public void BuildByType_WithSimpleProperties_ReturnsCorrectValues()
    {
        var entity = BuildTableEntity(new()
        {
            ["Name"] = "Dave",
            ["Age"] = 40,
            ["Active"] = false
        });

        var result = ObjectBuilder.BuildByType(typeof(SimplePoco), entity) as SimplePoco;

        Assert.NotNull(result);
        Assert.Equal("Dave", result!.Name);
        Assert.Equal(40, result.Age);
        Assert.False(result.Active);
    }
}
