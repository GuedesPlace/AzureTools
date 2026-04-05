using GuedesPlace.AzureTools.Configuration.Extensions;
using Microsoft.Extensions.Configuration;

namespace GuedesPlace.AzureTools.Tests.Configuration;

public class IConfigurationExtensionsTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    [Fact]
    public void CheckConfigurationValueAvailable_WithPresentKey_DoesNotThrow()
    {
        var config = BuildConfig(new() { ["MyKey"] = "some-value" });
        var ex = Record.Exception(() => config.CheckConfigurationValueAvailable("MyKey"));
        Assert.Null(ex);
    }

    [Fact]
    public void CheckConfigurationValueAvailable_WithMissingKey_ThrowsArgumentNullException()
    {
        var config = BuildConfig(new());
        var ex = Assert.Throws<ArgumentNullException>(() =>
            config.CheckConfigurationValueAvailable("MissingKey"));
        Assert.Equal("MissingKey", ex.ParamName);
    }

    [Fact]
    public void CheckConfigurationValueAvailable_WithEmptyValue_ThrowsArgumentNullException()
    {
        var config = BuildConfig(new() { ["EmptyKey"] = "" });
        var ex = Assert.Throws<ArgumentNullException>(() =>
            config.CheckConfigurationValueAvailable("EmptyKey"));
        Assert.Equal("EmptyKey", ex.ParamName);
    }

    [Fact]
    public void CheckConfigurationValueAvailable_WithNullValue_ThrowsArgumentNullException()
    {
        var config = BuildConfig(new() { ["NullKey"] = null });
        var ex = Assert.Throws<ArgumentNullException>(() =>
            config.CheckConfigurationValueAvailable("NullKey"));
        Assert.Equal("NullKey", ex.ParamName);
    }

    [Fact]
    public void CheckConfigurationValuesAvailable_WithAllKeysPresent_DoesNotThrow()
    {
        var config = BuildConfig(new()
        {
            ["Key1"] = "value1",
            ["Key2"] = "value2",
            ["Key3"] = "value3"
        });

        var ex = Record.Exception(() =>
            config.CheckConfigurationValuesAvailable(new[] { "Key1", "Key2", "Key3" }));
        Assert.Null(ex);
    }

    [Fact]
    public void CheckConfigurationValuesAvailable_WithOneMissingKey_ThrowsArgumentNullException()
    {
        var config = BuildConfig(new()
        {
            ["Key1"] = "value1",
            ["Key3"] = "value3"
            // Key2 is missing
        });

        var ex = Assert.Throws<ArgumentNullException>(() =>
            config.CheckConfigurationValuesAvailable(new[] { "Key1", "Key2", "Key3" }));
        Assert.Equal("Key2", ex.ParamName);
    }

    [Fact]
    public void CheckConfigurationValuesAvailable_WithEmptyCollection_DoesNotThrow()
    {
        var config = BuildConfig(new());
        var ex = Record.Exception(() =>
            config.CheckConfigurationValuesAvailable(Array.Empty<string>()));
        Assert.Null(ex);
    }
}
