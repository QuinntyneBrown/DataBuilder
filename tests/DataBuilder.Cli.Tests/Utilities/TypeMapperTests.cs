using System.Text.Json;
using DataBuilder.Cli.Utilities;
using FluentAssertions;

namespace DataBuilder.Cli.Tests.Utilities;

public class TypeMapperTests
{
    private static JsonElement ParseJson(string json)
    {
        return JsonDocument.Parse(json).RootElement;
    }

    #region ToCSharpType Tests

    [Fact]
    public void ToCSharpType_WithString_ReturnsString()
    {
        var element = ParseJson("\"hello\"");
        TypeMapper.ToCSharpType(element).Should().Be("string");
    }

    [Fact]
    public void ToCSharpType_WithEmptyString_ReturnsString()
    {
        var element = ParseJson("\"\"");
        TypeMapper.ToCSharpType(element).Should().Be("string");
    }

    [Fact]
    public void ToCSharpType_WithGuidString_ReturnsGuid()
    {
        var element = ParseJson("\"550e8400-e29b-41d4-a716-446655440000\"");
        TypeMapper.ToCSharpType(element).Should().Be("Guid");
    }

    [Fact]
    public void ToCSharpType_WithDateTimeString_ReturnsDateTime()
    {
        var element = ParseJson("\"2024-01-15T10:30:00Z\"");
        TypeMapper.ToCSharpType(element).Should().Be("DateTime");
    }

    [Fact]
    public void ToCSharpType_WithDateString_ReturnsDateTime()
    {
        var element = ParseJson("\"2024-12-31\"");
        TypeMapper.ToCSharpType(element).Should().Be("DateTime");
    }

    [Fact]
    public void ToCSharpType_WithInteger_ReturnsInt()
    {
        var element = ParseJson("42");
        TypeMapper.ToCSharpType(element).Should().Be("int");
    }

    [Fact]
    public void ToCSharpType_WithLargeInteger_ReturnsLong()
    {
        var element = ParseJson("9999999999999");
        TypeMapper.ToCSharpType(element).Should().Be("long");
    }

    [Fact]
    public void ToCSharpType_WithDecimal_ReturnsDecimal()
    {
        var element = ParseJson("99.99");
        TypeMapper.ToCSharpType(element).Should().Be("decimal");
    }

    [Fact]
    public void ToCSharpType_WithTrue_ReturnsBool()
    {
        var element = ParseJson("true");
        TypeMapper.ToCSharpType(element).Should().Be("bool");
    }

    [Fact]
    public void ToCSharpType_WithFalse_ReturnsBool()
    {
        var element = ParseJson("false");
        TypeMapper.ToCSharpType(element).Should().Be("bool");
    }

    [Fact]
    public void ToCSharpType_WithObject_ReturnsDictionary()
    {
        var element = ParseJson("{\"key\": \"value\"}");
        TypeMapper.ToCSharpType(element).Should().Be("Dictionary<string, object>");
    }

    [Fact]
    public void ToCSharpType_WithNull_ReturnsNullableString()
    {
        var element = ParseJson("null");
        TypeMapper.ToCSharpType(element).Should().Be("string?");
    }

    [Fact]
    public void ToCSharpType_WithEmptyArray_ReturnsListOfObject()
    {
        var element = ParseJson("[]");
        TypeMapper.ToCSharpType(element).Should().Be("List<object>");
    }

    [Fact]
    public void ToCSharpType_WithStringArray_ReturnsListOfString()
    {
        var element = ParseJson("[\"a\", \"b\", \"c\"]");
        TypeMapper.ToCSharpType(element).Should().Be("List<string>");
    }

    [Fact]
    public void ToCSharpType_WithIntArray_ReturnsListOfInt()
    {
        var element = ParseJson("[1, 2, 3]");
        TypeMapper.ToCSharpType(element).Should().Be("List<int>");
    }

    [Fact]
    public void ToCSharpType_WithObjectArray_ReturnsListOfDictionary()
    {
        var element = ParseJson("[{\"a\": 1}, {\"b\": 2}]");
        TypeMapper.ToCSharpType(element).Should().Be("List<Dictionary<string, object>>");
    }

    #endregion

    #region ToTypeScriptType Tests

    [Fact]
    public void ToTypeScriptType_WithString_ReturnsString()
    {
        var element = ParseJson("\"hello\"");
        TypeMapper.ToTypeScriptType(element).Should().Be("string");
    }

    [Fact]
    public void ToTypeScriptType_WithEmptyString_ReturnsString()
    {
        var element = ParseJson("\"\"");
        TypeMapper.ToTypeScriptType(element).Should().Be("string");
    }

    [Fact]
    public void ToTypeScriptType_WithDateTimeString_ReturnsDate()
    {
        var element = ParseJson("\"2024-01-15T10:30:00Z\"");
        TypeMapper.ToTypeScriptType(element).Should().Be("Date");
    }

    [Fact]
    public void ToTypeScriptType_WithInteger_ReturnsNumber()
    {
        var element = ParseJson("42");
        TypeMapper.ToTypeScriptType(element).Should().Be("number");
    }

    [Fact]
    public void ToTypeScriptType_WithDecimal_ReturnsNumber()
    {
        var element = ParseJson("99.99");
        TypeMapper.ToTypeScriptType(element).Should().Be("number");
    }

    [Fact]
    public void ToTypeScriptType_WithTrue_ReturnsBoolean()
    {
        var element = ParseJson("true");
        TypeMapper.ToTypeScriptType(element).Should().Be("boolean");
    }

    [Fact]
    public void ToTypeScriptType_WithFalse_ReturnsBoolean()
    {
        var element = ParseJson("false");
        TypeMapper.ToTypeScriptType(element).Should().Be("boolean");
    }

    [Fact]
    public void ToTypeScriptType_WithObject_ReturnsRecord()
    {
        var element = ParseJson("{\"key\": \"value\"}");
        TypeMapper.ToTypeScriptType(element).Should().Be("Record<string, any>");
    }

    [Fact]
    public void ToTypeScriptType_WithNull_ReturnsNullableString()
    {
        var element = ParseJson("null");
        TypeMapper.ToTypeScriptType(element).Should().Be("string | null");
    }

    [Fact]
    public void ToTypeScriptType_WithEmptyArray_ReturnsAnyArray()
    {
        var element = ParseJson("[]");
        TypeMapper.ToTypeScriptType(element).Should().Be("any[]");
    }

    [Fact]
    public void ToTypeScriptType_WithStringArray_ReturnsStringArray()
    {
        var element = ParseJson("[\"a\", \"b\", \"c\"]");
        TypeMapper.ToTypeScriptType(element).Should().Be("string[]");
    }

    [Fact]
    public void ToTypeScriptType_WithNumberArray_ReturnsNumberArray()
    {
        var element = ParseJson("[1, 2, 3]");
        TypeMapper.ToTypeScriptType(element).Should().Be("number[]");
    }

    #endregion

    #region CSharpToTypeScript Tests

    [Theory]
    [InlineData("string", "string")]
    [InlineData("int", "number")]
    [InlineData("long", "number")]
    [InlineData("double", "number")]
    [InlineData("decimal", "number")]
    [InlineData("float", "number")]
    [InlineData("bool", "boolean")]
    [InlineData("DateTime", "Date")]
    [InlineData("DateOnly", "Date")]
    [InlineData("TimeOnly", "string")]
    [InlineData("Guid", "string")]
    [InlineData("object", "any")]
    [InlineData("Dictionary<string, object>", "Record<string, any>")]
    public void CSharpToTypeScript_ShouldMapCorrectly(string csharpType, string expectedTsType)
    {
        TypeMapper.CSharpToTypeScript(csharpType).Should().Be(expectedTsType);
    }

    [Theory]
    [InlineData("string?", "string | null")]
    [InlineData("int?", "number | null")]
    [InlineData("bool?", "boolean | null")]
    [InlineData("DateTime?", "Date | null")]
    public void CSharpToTypeScript_WithNullable_ShouldIncludeNull(string csharpType, string expectedTsType)
    {
        TypeMapper.CSharpToTypeScript(csharpType).Should().Be(expectedTsType);
    }

    [Theory]
    [InlineData("List<string>", "string[]")]
    [InlineData("List<int>", "number[]")]
    [InlineData("List<bool>", "boolean[]")]
    [InlineData("List<DateTime>", "Date[]")]
    public void CSharpToTypeScript_WithList_ShouldReturnArray(string csharpType, string expectedTsType)
    {
        TypeMapper.CSharpToTypeScript(csharpType).Should().Be(expectedTsType);
    }

    [Theory]
    [InlineData("string[]", "string[]")]
    [InlineData("int[]", "number[]")]
    public void CSharpToTypeScript_WithArray_ShouldReturnArray(string csharpType, string expectedTsType)
    {
        TypeMapper.CSharpToTypeScript(csharpType).Should().Be(expectedTsType);
    }

    [Fact]
    public void CSharpToTypeScript_WithUnknownType_ShouldReturnAny()
    {
        TypeMapper.CSharpToTypeScript("CustomType").Should().Be("any");
    }

    #endregion
}
