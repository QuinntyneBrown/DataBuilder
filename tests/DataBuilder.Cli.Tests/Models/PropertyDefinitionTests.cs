using DataBuilder.Cli.Models;
using FluentAssertions;

namespace DataBuilder.Cli.Tests.Models;

public class PropertyDefinitionTests
{
    [Fact]
    public void IsRequired_WhenNotNullable_ReturnsTrue()
    {
        var property = new PropertyDefinition { IsNullable = false };
        property.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void IsRequired_WhenNullable_ReturnsFalse()
    {
        var property = new PropertyDefinition { IsNullable = true };
        property.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var property = new PropertyDefinition();

        property.Name.Should().BeEmpty();
        property.NameCamelCase.Should().BeEmpty();
        property.CSharpType.Should().BeEmpty();
        property.TypeScriptType.Should().BeEmpty();
        property.IsNullable.Should().BeFalse();
        property.IsCollection.Should().BeFalse();
        property.IsId.Should().BeFalse();
        property.IsObject.Should().BeFalse();
        property.SampleValue.Should().BeNull();
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        var property = new PropertyDefinition
        {
            Name = "ProductName",
            NameCamelCase = "productName",
            CSharpType = "string",
            TypeScriptType = "string",
            IsNullable = true,
            IsCollection = false,
            IsId = false,
            IsObject = false,
            SampleValue = "Sample"
        };

        property.Name.Should().Be("ProductName");
        property.NameCamelCase.Should().Be("productName");
        property.CSharpType.Should().Be("string");
        property.TypeScriptType.Should().Be("string");
        property.IsNullable.Should().BeTrue();
        property.IsRequired.Should().BeFalse();
        property.IsCollection.Should().BeFalse();
        property.IsId.Should().BeFalse();
        property.IsObject.Should().BeFalse();
        property.SampleValue.Should().Be("Sample");
    }

    [Fact]
    public void CollectionProperty_ShouldBeConfigurable()
    {
        var property = new PropertyDefinition
        {
            Name = "Tags",
            CSharpType = "List<string>",
            TypeScriptType = "string[]",
            IsCollection = true
        };

        property.IsCollection.Should().BeTrue();
        property.CSharpType.Should().Be("List<string>");
        property.TypeScriptType.Should().Be("string[]");
    }

    [Fact]
    public void ObjectProperty_ShouldBeConfigurable()
    {
        var property = new PropertyDefinition
        {
            Name = "Metadata",
            CSharpType = "Dictionary<string, object>",
            TypeScriptType = "Record<string, any>",
            IsObject = true
        };

        property.IsObject.Should().BeTrue();
        property.CSharpType.Should().Be("Dictionary<string, object>");
        property.TypeScriptType.Should().Be("Record<string, any>");
    }

    [Fact]
    public void IdProperty_ShouldBeConfigurable()
    {
        var property = new PropertyDefinition
        {
            Name = "Id",
            NameCamelCase = "id",
            CSharpType = "string",
            TypeScriptType = "string",
            IsId = true
        };

        property.IsId.Should().BeTrue();
        property.Name.Should().Be("Id");
    }
}
