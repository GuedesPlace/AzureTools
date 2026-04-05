using GuedesPlace.AzureTools.Tables.Converters;

namespace GuedesPlace.AzureTools.Tests.Converters;

public class EnumerableConverterTests
{
    private readonly EnumerableConverter _sut = new();

    [Fact]
    public void IsType_WithListOfInt_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(List<int>)));
    }

    [Fact]
    public void IsType_WithListOfString_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(List<string>)));
    }

    [Fact]
    public void IsType_WithIEnumerableOfInt_ReturnsFalse()
    {
        // IEnumerable<T> itself does not appear in its own GetInterfaces() result.
        // The converter handles concrete implementations (e.g. List<T>), not the interface itself.
        Assert.False(_sut.IsType(typeof(IEnumerable<int>)));
    }

    [Fact]
    public void IsType_WithString_ReturnsFalse()
    {
        // String implements IEnumerable<char> but is explicitly excluded
        Assert.False(_sut.IsType(typeof(string)));
    }

    [Fact]
    public void IsType_WithByteArray_ReturnsFalse()
    {
        // byte[] is explicitly excluded
        Assert.False(_sut.IsType(typeof(byte[])));
    }

    [Fact]
    public void IsType_WithInt_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(int)));
    }

    [Fact]
    public void GetValue_WithListOfInt_ReturnsJsonString()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = _sut.GetValue(typeof(List<int>), list);
        Assert.Equal("[1,2,3]", result);
    }

    [Fact]
    public void BuildValue_WithValidJson_ReturnsList()
    {
        var result = _sut.BuildValue("[1,2,3]", typeof(List<int>)) as List<int>;
        Assert.NotNull(result);
        Assert.Equal(new List<int> { 1, 2, 3 }, result);
    }

    [Fact]
    public void BuildValue_WithNull_ReturnsNull()
    {
        var result = _sut.BuildValue(null, typeof(List<int>));
        Assert.Null(result);
    }

    [Fact]
    public void BuildValue_WithEmptyString_ReturnsNull()
    {
        var result = _sut.BuildValue("", typeof(List<int>));
        Assert.Null(result);
    }

    [Fact]
    public void RoundTrip_ListOfString_PreservesValues()
    {
        var original = new List<string> { "alpha", "beta", "gamma" };
        var serialized = _sut.GetValue(typeof(List<string>), original);
        var deserialized = _sut.BuildValue(serialized, typeof(List<string>)) as List<string>;
        Assert.NotNull(deserialized);
        Assert.Equal(original, deserialized);
    }
}
