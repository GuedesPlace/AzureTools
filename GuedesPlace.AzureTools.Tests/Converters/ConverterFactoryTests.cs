using GuedesPlace.AzureTools.Tables.Converters;

namespace GuedesPlace.AzureTools.Tests.Converters;

public class ConverterFactoryTests
{
    private enum SampleEnum { A, B }

    [Fact]
    public void FindConverter_ForEnumType_ReturnsEnumConverter()
    {
        var converter = ConverterFactory.FindConverter(typeof(SampleEnum));
        Assert.NotNull(converter);
        Assert.IsType<EnumConverter>(converter);
    }

    [Fact]
    public void FindConverter_ForTimeSpan_ReturnsTimeSpanConverter()
    {
        var converter = ConverterFactory.FindConverter(typeof(TimeSpan));
        Assert.NotNull(converter);
        Assert.IsType<TimeSpanConverter>(converter);
    }

    [Fact]
    public void FindConverter_ForNullableTimeSpan_ReturnsTimeSpanConverter()
    {
        var converter = ConverterFactory.FindConverter(typeof(TimeSpan?));
        Assert.NotNull(converter);
        Assert.IsType<TimeSpanConverter>(converter);
    }

    [Fact]
    public void FindConverter_ForIntArray_ReturnsArrayConverter()
    {
        var converter = ConverterFactory.FindConverter(typeof(int[]));
        Assert.NotNull(converter);
        Assert.IsType<ArrayConverter>(converter);
    }

    [Fact]
    public void FindConverter_ForStringArray_ReturnsArrayConverterNotEnumerable()
    {
        // Arrays should match ArrayConverter before EnumerableConverter
        var converter = ConverterFactory.FindConverter(typeof(string[]));
        Assert.NotNull(converter);
        Assert.IsType<ArrayConverter>(converter);
    }

    [Fact]
    public void FindConverter_ForListOfInt_ReturnsEnumerableConverter()
    {
        var converter = ConverterFactory.FindConverter(typeof(List<int>));
        Assert.NotNull(converter);
        Assert.IsType<EnumerableConverter>(converter);
    }

    [Fact]
    public void FindConverter_ForString_ReturnsNull()
    {
        // string is a primitive-like pass-through; no converter
        var converter = ConverterFactory.FindConverter(typeof(string));
        Assert.Null(converter);
    }

    [Fact]
    public void FindConverter_ForInt_ReturnsNull()
    {
        var converter = ConverterFactory.FindConverter(typeof(int));
        Assert.Null(converter);
    }

    [Fact]
    public void FindConverter_ForByteArray_ReturnsNull()
    {
        // byte[] is explicitly excluded from both ArrayConverter and EnumerableConverter
        var converter = ConverterFactory.FindConverter(typeof(byte[]));
        Assert.Null(converter);
    }

    [Fact]
    public void FindConverter_CalledTwice_ReturnsSameConverterType()
    {
        // Verifies that the static lazy initialization returns consistent results
        var first = ConverterFactory.FindConverter(typeof(TimeSpan));
        var second = ConverterFactory.FindConverter(typeof(TimeSpan));
        Assert.Equal(first!.GetType(), second!.GetType());
    }
}
