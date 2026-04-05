using Azure;
using Azure.Data.Tables;
using GuedesPlace.AzureTools.Tables.Models;

namespace GuedesPlace.AzureTools.Tests.Tables;

public class TableEntityResultTests
{
    private class SampleEntity
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    private static TableEntity BuildTableEntity(string partitionKey, string rowKey, Dictionary<string, object?> values)
    {
        var entity = new TableEntity(partitionKey, rowKey);
        entity.ETag = new ETag("etag-value");
        entity.Timestamp = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
        foreach (var kv in values)
        {
            entity[kv.Key] = kv.Value;
        }
        return entity;
    }

    [Fact]
    public void BuildTableEntityResult_CopiesRowKey()
    {
        var entity = BuildTableEntity("pk1", "rk1", new() { ["Name"] = "Alice", ["Value"] = 99 });
        var result = TableEntityResult<SampleEntity>.BuildTableEntityResult<SampleEntity>(entity);
        Assert.Equal("rk1", result.RowKey);
    }

    [Fact]
    public void BuildTableEntityResult_CopiesPartitionKey()
    {
        var entity = BuildTableEntity("pk1", "rk1", new() { ["Name"] = "Bob", ["Value"] = 1 });
        var result = TableEntityResult<SampleEntity>.BuildTableEntityResult<SampleEntity>(entity);
        Assert.Equal("pk1", result.PartitionKey);
    }

    [Fact]
    public void BuildTableEntityResult_CopiesETag()
    {
        var entity = BuildTableEntity("pk1", "rk1", new() { ["Name"] = "Carol", ["Value"] = 2 });
        var result = TableEntityResult<SampleEntity>.BuildTableEntityResult<SampleEntity>(entity);
        Assert.Equal(new ETag("etag-value"), result.ETag);
    }

    [Fact]
    public void BuildTableEntityResult_CopiesTimestamp()
    {
        var expected = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var entity = BuildTableEntity("pk1", "rk1", new() { ["Name"] = "Dave", ["Value"] = 3 });
        var result = TableEntityResult<SampleEntity>.BuildTableEntityResult<SampleEntity>(entity);
        Assert.Equal(expected, result.Timestamp);
    }

    [Fact]
    public void BuildTableEntityResult_DeserializesEntity()
    {
        var entity = BuildTableEntity("pk1", "rk1", new() { ["Name"] = "Eve", ["Value"] = 42 });
        var result = TableEntityResult<SampleEntity>.BuildTableEntityResult<SampleEntity>(entity);

        Assert.NotNull(result.Entity);
        Assert.Equal("Eve", result.Entity.Name);
        Assert.Equal(42, result.Entity.Value);
    }

    [Fact]
    public void BuildTableEntityResultWithType_DeserializesEntity()
    {
        var entity = BuildTableEntity("pk1", "rk1", new() { ["Name"] = "Frank", ["Value"] = 7 });
        var result = TableEntityResult<object>.BuildTableEntityResultWithType(typeof(SampleEntity), entity);

        Assert.NotNull(result.Entity);
        var typed = result.Entity as SampleEntity;
        Assert.NotNull(typed);
        Assert.Equal("Frank", typed!.Name);
        Assert.Equal(7, typed.Value);
    }

    [Fact]
    public void BuildTableEntityResultWithType_CopiesMetadata()
    {
        var entity = BuildTableEntity("partA", "rowB", new() { ["Name"] = "Grace", ["Value"] = 0 });
        var result = TableEntityResult<object>.BuildTableEntityResultWithType(typeof(SampleEntity), entity);

        Assert.Equal("partA", result.PartitionKey);
        Assert.Equal("rowB", result.RowKey);
    }
}
