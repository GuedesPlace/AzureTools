using GuedesPlace.AzureTools.Tables;

namespace GuedesPlace.AzureTools.Tests.Tables;

public class ObjectSerializerTests
{
    private enum Color { Red, Green, Blue }

    private class SimplePoco
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public bool Active { get; set; }
    }

    private class PocoWithNullable
    {
        public string? NullableString { get; set; }
        public int? NullableInt { get; set; }
    }

    private class PocoWithBytes
    {
        public byte[]? Data { get; set; }
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

    private class PocoWithEnum
    {
        public Color FavoriteColor { get; set; }
    }

    private class PocoWithTimeSpan
    {
        public TimeSpan Duration { get; set; }
    }

    private class PocoWithReadOnlyProperty
    {
        public string? ReadWrite { get; set; }
        public string ReadOnly { get; } = "readonly";
    }

    [Fact]
    public void Serialize_SimplePoco_ProducesCorrectKeys()
    {
        var obj = new SimplePoco { Name = "Alice", Age = 30, Active = true };
        var result = ObjectSerializer.Serialize(obj);

        Assert.True(result.ContainsKey("Name"));
        Assert.True(result.ContainsKey("Age"));
        Assert.True(result.ContainsKey("Active"));
        Assert.Equal("Alice", result["Name"]);
        Assert.Equal(30, result["Age"]);
        Assert.Equal(true, result["Active"]);
    }

    [Fact]
    public void Serialize_NullProperty_IsNotIncluded()
    {
        var obj = new PocoWithNullable { NullableString = null, NullableInt = 42 };
        var result = ObjectSerializer.Serialize(obj);

        Assert.False(result.ContainsKey("NullableString"));
        Assert.True(result.ContainsKey("NullableInt"));
    }

    [Fact]
    public void Serialize_ByteArray_PassesThroughDirectly()
    {
        var data = new byte[] { 1, 2, 3 };
        var obj = new PocoWithBytes { Data = data };
        var result = ObjectSerializer.Serialize(obj);

        Assert.True(result.ContainsKey("Data"));
        Assert.Equal(data, result["Data"]);
    }

    [Fact]
    public void Serialize_NestedPoco_ProducesUnderscoreDelimitedKeys()
    {
        var obj = new NestedPoco
        {
            Title = "Test",
            Address = new Address { City = "Berlin", Street = "Hauptstrasse" }
        };
        var result = ObjectSerializer.Serialize(obj);

        Assert.True(result.ContainsKey("Title"));
        Assert.True(result.ContainsKey("Address_City"));
        Assert.True(result.ContainsKey("Address_Street"));
        Assert.Equal("Berlin", result["Address_City"]);
        Assert.Equal("Hauptstrasse", result["Address_Street"]);
    }

    [Fact]
    public void Serialize_ReadOnlyProperty_IsNotIncluded()
    {
        var obj = new PocoWithReadOnlyProperty { ReadWrite = "rw" };
        var result = ObjectSerializer.Serialize(obj);

        Assert.True(result.ContainsKey("ReadWrite"));
        Assert.False(result.ContainsKey("ReadOnly"));
    }

    // ─── Bug-documenting tests ───────────────────────────────────────────────
    // ObjectSerializer.cs calls: converter.GetValue(propertyInfo.GetType(), value)
    // propertyInfo.GetType() always returns System.Reflection.PropertyInfo, NOT
    // the property's declared type. This is a latent bug: the wrong type is
    // passed to converter.GetValue(), but all existing GetValue() implementations
    // ignore the `type` parameter and simply call value.ToString(). Therefore the
    // output is accidentally correct today. The bug would only manifest if a
    // future converter.GetValue() implementation actually uses the `type` param.

    [Fact]
    public void Serialize_EnumProperty_BugDocumentation_WrongTypePassedToGetValue_OutputAccidentallyCorrect()
    {
        // LATENT BUG: converter.GetValue(propertyInfo.GetType(), value) passes
        // typeof(PropertyInfo) instead of typeof(Color) / propertyInfo.PropertyType.
        // EnumConverter.GetValue ignores its `type` param and calls value.ToString(),
        // so the result is "Green" — correct despite the wrong argument.
        // Fix would be: converter.GetValue(propertyInfo.PropertyType, value)
        var obj = new PocoWithEnum { FavoriteColor = Color.Green };
        var result = ObjectSerializer.Serialize(obj);

        Assert.True(result.ContainsKey("FavoriteColor"));
        // Output is correct (string "Green") despite the wrong type being passed
        Assert.Equal("Green", result["FavoriteColor"]);
    }

    [Fact]
    public void Serialize_TimeSpanProperty_BugDocumentation_WrongTypePassedToGetValue_OutputAccidentallyCorrect()
    {
        // LATENT BUG: Same as above. TimeSpanConverter.GetValue ignores its `type`
        // param and calls value.ToString(), so the result is the correct string
        // despite propertyInfo.GetType() (= PropertyInfo) being passed instead of
        // propertyInfo.PropertyType (= TimeSpan).
        // Fix would be: converter.GetValue(propertyInfo.PropertyType, value)
        var duration = TimeSpan.FromHours(1.5);
        var obj = new PocoWithTimeSpan { Duration = duration };
        var result = ObjectSerializer.Serialize(obj);

        Assert.True(result.ContainsKey("Duration"));
        // Output is correct (string "01:30:00") despite the wrong type being passed
        Assert.Equal("01:30:00", result["Duration"]);
    }
}
