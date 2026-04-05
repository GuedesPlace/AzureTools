using GuedesPlace.AzureTools.Tables.Converters;

namespace GuedesPlace.AzureTools.Tests.Converters;

public class TimeSpanConverterTests
{
    private readonly TimeSpanConverter _sut = new();

    [Fact]
    public void IsType_WithTimeSpan_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(TimeSpan)));
    }

    [Fact]
    public void IsType_WithNullableTimeSpan_ReturnsTrue()
    {
        Assert.True(_sut.IsType(typeof(TimeSpan?)));
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
    public void IsType_WithDateTime_ReturnsFalse()
    {
        Assert.False(_sut.IsType(typeof(DateTime)));
    }

    [Fact]
    public void GetValue_WithTimeSpan_ReturnsString()
    {
        var ts = TimeSpan.FromHours(2.5);
        var result = _sut.GetValue(typeof(TimeSpan), ts);
        Assert.Equal(ts.ToString(), result);
    }

    [Fact]
    public void GetValue_WithZero_ReturnsZeroString()
    {
        var result = _sut.GetValue(typeof(TimeSpan), TimeSpan.Zero);
        Assert.Equal("00:00:00", result);
    }

    [Fact]
    public void GetValue_WithNegativeTimeSpan_ReturnsString()
    {
        var ts = TimeSpan.FromMinutes(-90);
        var result = _sut.GetValue(typeof(TimeSpan), ts);
        Assert.Equal(ts.ToString(), result);
    }

    [Fact]
    public void BuildValue_WithValidString_ReturnsTimeSpan()
    {
        var result = _sut.BuildValue("02:30:00", typeof(TimeSpan));
        Assert.Equal(TimeSpan.FromHours(2.5), result);
    }

    [Fact]
    public void BuildValue_WithNull_ReturnsNull()
    {
        var result = _sut.BuildValue(null, typeof(TimeSpan));
        Assert.Null(result);
    }

    [Fact]
    public void BuildValue_WithEmptyString_ReturnsNull()
    {
        var result = _sut.BuildValue("", typeof(TimeSpan));
        Assert.Null(result);
    }

    [Fact]
    public void BuildValue_WithDaysAndTime_ReturnsParsedValue()
    {
        var result = _sut.BuildValue("1.02:30:00", typeof(TimeSpan));
        Assert.Equal(new TimeSpan(1, 2, 30, 0), result);
    }

    [Fact]
    public void RoundTrip_TimeSpan_PreservesValue()
    {
        var original = new TimeSpan(3, 14, 15, 9);
        var serialized = _sut.GetValue(typeof(TimeSpan), original);
        var deserialized = (TimeSpan)_sut.BuildValue(serialized, typeof(TimeSpan))!;
        Assert.Equal(original, deserialized);
    }
}
