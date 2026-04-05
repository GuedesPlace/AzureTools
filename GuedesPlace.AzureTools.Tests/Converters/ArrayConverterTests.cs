using GuedesPlace.AzureTools.Tables.Converters;
using Newtonsoft.Json;

namespace GuedesPlace.AzureTools.Tests.Converters;

public class ArrayConverterTests
{
    private readonly ArrayConverter _sut = new();

    [Fact]
    public void IsType_WithIntArray_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(int[])));
    }

    [Fact]
    public void IsType_WithStringArray_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(string[])));
    }

    [Fact]
    public void IsType_WithByteArray_ReturnsFalse()
    {
        // byte[] is explicitly excluded
        Assert.False(_sut.IsType(typeof(byte[])));
    }

    [Fact]
    public void IsType_WithString_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(string)));
    }

    [Fact]
    public void IsType_WithList_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(List<int>)));
    }

    [Fact]
    public void IsType_WithInt_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(int)));
    }

    [Fact]
    public void GetValue_WithIntArray_ReturnsJsonString()
    {
        var array = new[] { 1, 2, 3 };
        var result = _sut.GetValue(typeof(int[]), array);
        Assert.Equal(JsonConvert.SerializeObject(array), result);
    }

    [Fact]
    public void GetValue_WithStringArray_ReturnsJsonString()
    {
        var array = new[] { "a", "b", "c" };
        var result = _sut.GetValue(typeof(string[]), array);
        Assert.Equal("[\"a\",\"b\",\"c\"]", result);
    }

    [Fact]
    public void BuildValue_WithValidJson_ReturnsArray()
    {
        var result = _sut.BuildValue("[1,2,3]", typeof(int[]));
        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void BuildValue_WithNull_ReturnsNull()
    {
        var result = _sut.BuildValue(null, typeof(int[]));
        Assert.Null(result);
    }

    [Fact]
    public void BuildValue_WithEmptyString_ReturnsNull()
    {
        var result = _sut.BuildValue("", typeof(int[]));
        Assert.Null(result);
    }

    [Fact]
    public void RoundTrip_IntArray_PreservesValues()
    {
        var original = new[] { 10, 20, 30 };
        var serialized = _sut.GetValue(typeof(int[]), original);
        var deserialized = (int[])_sut.BuildValue(serialized, typeof(int[]))!;
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void RoundTrip_StringArray_PreservesValues()
    {
        var original = new[] { "hello", "world" };
        var serialized = _sut.GetValue(typeof(string[]), original);
        var deserialized = (string[])_sut.BuildValue(serialized, typeof(string[]))!;
        Assert.Equal(original, deserialized);
    }
}
