using GuedesPlace.AzureTools.Tables.Converters;

namespace GuedesPlace.AzureTools.Tests.Converters;

public class EnumConverterTests
{
    private readonly EnumConverter _sut = new();

    private enum Color { Red, Green, Blue }
    private enum Status { Active = 1, Inactive = 2 }

    [Fact]
    public void IsType_WithEnumType_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(Color)));
    }

    [Fact]
    public void IsType_WithAnotherEnumType_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(Status)));
    }

    [Fact]
    public void IsType_WithString_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(string)));
    }

    [Fact]
    public void IsType_WithInt_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(int)));
    }

    [Fact]
    public void IsType_WithClass_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(object)));
    }

    [Fact]
    public void GetValue_WithEnumValue_ReturnsStringRepresentation()
    {
        var result = _sut.GetValue(typeof(Color), Color.Green);
        Assert.Equal("Green", result);
    }

    [Fact]
    public void GetValue_WithNumericEnumValue_ReturnsName()
    {
        var result = _sut.GetValue(typeof(Status), Status.Inactive);
        Assert.Equal("Inactive", result);
    }

    [Fact]
    public void BuildValue_WithValidString_ReturnsEnumValue()
    {
        var result = _sut.BuildValue("Green", typeof(Color));
        Assert.Equal(Color.Green, result);
    }

    [Fact]
    public void BuildValue_WithNull_ReturnsNull()
    {
        var result = _sut.BuildValue(null, typeof(Color));
        Assert.Null(result);
    }

    [Fact]
    public void BuildValue_WithEmptyString_ReturnsNull()
    {
        var result = _sut.BuildValue("", typeof(Color));
        Assert.Null(result);
    }

    [Fact]
    public void RoundTrip_EnumValue_PreservesValue()
    {
        var original = Color.Blue;
        var serialized = _sut.GetValue(typeof(Color), original);
        var deserialized = _sut.BuildValue(serialized, typeof(Color));
        Assert.Equal(original, deserialized);
    }
}
