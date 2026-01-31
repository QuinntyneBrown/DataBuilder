using DataBuilder.Cli.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataBuilder.Cli.Tests.Services;

public class SchemaParserTests
{
    private readonly Mock<ILogger<SchemaParser>> _loggerMock;
    private readonly SchemaParser _parser;

    public SchemaParserTests()
    {
        _loggerMock = new Mock<ILogger<SchemaParser>>();
        _parser = new SchemaParser(_loggerMock.Object);
    }

    #region Basic Parsing Tests

    [Fact]
    public void Parse_SimpleEntity_ReturnsEntityDefinition()
    {
        var json = @"{
            ""product"": {
                ""name"": ""Sample"",
                ""price"": 99.99
            }
        }";

        var entities = _parser.Parse(json);

        entities.Should().HaveCount(1);
        entities[0].Name.Should().Be("Product");
        entities[0].Properties.Should().Contain(p => p.Name == "Name");
        entities[0].Properties.Should().Contain(p => p.Name == "Price");
    }

    [Fact]
    public void Parse_MultipleEntities_ReturnsAllEntities()
    {
        var json = @"{
            ""product"": {
                ""name"": ""Sample""
            },
            ""category"": {
                ""title"": ""Electronics""
            }
        }";

        var entities = _parser.Parse(json);

        entities.Should().HaveCount(2);
        entities.Select(e => e.Name).Should().BeEquivalentTo(new[] { "Product", "Category" });
    }

    [Fact]
    public void Parse_EmptyJson_ReturnsEmptyList()
    {
        var json = "{}";

        var entities = _parser.Parse(json);

        entities.Should().BeEmpty();
    }

    #endregion

    #region Property Type Inference Tests

    [Fact]
    public void Parse_StringProperty_InfersStringType()
    {
        var json = @"{ ""product"": { ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);
        var nameProp = entities[0].Properties.First(p => p.Name == "Name");

        nameProp.CSharpType.Should().Be("string");
        nameProp.TypeScriptType.Should().Be("string");
    }

    [Fact]
    public void Parse_IntegerProperty_InfersIntType()
    {
        var json = @"{ ""product"": { ""quantity"": 10 } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "Quantity");

        prop.CSharpType.Should().Be("int");
        prop.TypeScriptType.Should().Be("number");
    }

    [Fact]
    public void Parse_DecimalProperty_InfersDecimalType()
    {
        var json = @"{ ""product"": { ""price"": 99.99 } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "Price");

        prop.CSharpType.Should().Be("decimal");
        prop.TypeScriptType.Should().Be("number");
    }

    [Fact]
    public void Parse_BooleanProperty_InfersBoolType()
    {
        var json = @"{ ""product"": { ""isActive"": true } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "IsActive");

        prop.CSharpType.Should().Be("bool");
        prop.TypeScriptType.Should().Be("boolean");
    }

    [Fact]
    public void Parse_DateTimeProperty_InfersDateTimeType()
    {
        var json = @"{ ""product"": { ""createdAt"": ""2024-01-15T10:30:00Z"" } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "CreatedAt");

        prop.CSharpType.Should().Be("DateTime");
        prop.TypeScriptType.Should().Be("Date");
    }

    [Fact]
    public void Parse_ObjectProperty_InfersDictionaryType()
    {
        var json = @"{ ""product"": { ""metadata"": { ""key"": ""value"" } } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "Metadata");

        prop.CSharpType.Should().Be("Dictionary<string, object>");
        prop.TypeScriptType.Should().Be("Record<string, any>");
        prop.IsObject.Should().BeTrue();
    }

    [Fact]
    public void Parse_ArrayProperty_InfersListType()
    {
        var json = @"{ ""product"": { ""tags"": [""a"", ""b""] } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "Tags");

        prop.CSharpType.Should().Be("List<string>");
        prop.TypeScriptType.Should().Be("string[]");
        prop.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void Parse_NullProperty_InfersNullableString()
    {
        var json = @"{ ""product"": { ""optionalField"": null } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "OptionalField");

        prop.CSharpType.Should().Be("string?");
        prop.IsNullable.Should().BeTrue();
    }

    #endregion

    #region ID Property Detection Tests

    [Fact]
    public void Parse_WithIdProperty_MarksAsId()
    {
        var json = @"{ ""product"": { ""id"": """", ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);
        var idProp = entities[0].Properties.First(p => p.Name == "Id");

        idProp.IsId.Should().BeTrue();
        idProp.CSharpType.Should().Be("string");
    }

    [Fact]
    public void Parse_WithEntityIdProperty_MarksAsId()
    {
        var json = @"{ ""product"": { ""productId"": """", ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);
        var idProp = entities[0].Properties.First(p => p.Name == "ProductId");

        idProp.IsId.Should().BeTrue();
        idProp.CSharpType.Should().Be("string");
    }

    [Fact]
    public void Parse_WithoutIdProperty_AddsIdProperty()
    {
        var json = @"{ ""product"": { ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);

        entities[0].Properties.Should().Contain(p => p.Name == "Id" && p.IsId);
        entities[0].IdProperty.Should().NotBeNull();
        entities[0].IdPropertyName.Should().Be("Id");
    }

    [Fact]
    public void Parse_AddedIdProperty_IsFirstInList()
    {
        var json = @"{ ""product"": { ""name"": ""Sample"", ""price"": 10 } }";

        var entities = _parser.Parse(json);

        entities[0].Properties[0].Name.Should().Be("Id");
        entities[0].Properties[0].IsId.Should().BeTrue();
    }

    #endregion

    #region Type Discriminator Tests

    [Fact]
    public void Parse_WithTypeProperty_EnablesTypeDiscriminator()
    {
        var json = @"{ ""product"": { ""type"": ""product"", ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);

        entities[0].UseTypeDiscriminator.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithTypeProperty_RemovesTypeFromProperties()
    {
        var json = @"{ ""product"": { ""type"": ""product"", ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);

        entities[0].Properties.Should().NotContain(p => p.Name == "Type");
    }

    [Fact]
    public void Parse_WithoutTypeProperty_DoesNotEnableTypeDiscriminator()
    {
        var json = @"{ ""product"": { ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);

        entities[0].UseTypeDiscriminator.Should().BeFalse();
    }

    #endregion

    #region Naming Convention Tests

    [Fact]
    public void Parse_CamelCasePropertyNames_ConvertedToPascalCase()
    {
        var json = @"{ ""product"": { ""productName"": ""Sample"", ""isActive"": true } }";

        var entities = _parser.Parse(json);

        entities[0].Properties.Should().Contain(p => p.Name == "ProductName");
        entities[0].Properties.Should().Contain(p => p.Name == "IsActive");
    }

    [Fact]
    public void Parse_CamelCasePropertyNames_PreservesCamelCaseVersion()
    {
        var json = @"{ ""product"": { ""productName"": ""Sample"" } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "ProductName");

        prop.NameCamelCase.Should().Be("productName");
    }

    [Fact]
    public void Parse_CamelCaseEntityName_ConvertedToPascalCase()
    {
        var json = @"{ ""productCategory"": { ""name"": ""Sample"" } }";

        var entities = _parser.Parse(json);

        entities[0].Name.Should().Be("ProductCategory");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Parse_InvalidJson_ThrowsException()
    {
        var json = "{ invalid json }";

        var action = () => _parser.Parse(json);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Invalid JSON:*");
    }

    [Fact]
    public void Parse_JsonArray_ThrowsException()
    {
        var json = "[1, 2, 3]";

        var action = () => _parser.Parse(json);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("JSON root must be an object*");
    }

    [Fact]
    public void Parse_EntityNotObject_ThrowsException()
    {
        var json = @"{ ""product"": ""not an object"" }";

        var action = () => _parser.Parse(json);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Entity 'product' must be defined as a JSON object*");
    }

    #endregion

    #region Sample Value Tests

    [Fact]
    public void Parse_StringProperty_StoresSampleValue()
    {
        var json = @"{ ""product"": { ""name"": ""Sample Product"" } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "Name");

        prop.SampleValue.Should().Be("Sample Product");
    }

    [Fact]
    public void Parse_NumberProperty_StoresSampleValue()
    {
        var json = @"{ ""product"": { ""price"": 99.99 } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "Price");

        prop.SampleValue.Should().Be("99.99");
    }

    [Fact]
    public void Parse_BooleanProperty_StoresSampleValue()
    {
        var json = @"{ ""product"": { ""isActive"": true } }";

        var entities = _parser.Parse(json);
        var prop = entities[0].Properties.First(p => p.Name == "IsActive");

        prop.SampleValue.Should().Be("true");
    }

    #endregion

    #region Complex Schema Tests

    [Fact]
    public void Parse_ComplexEntity_ParsesAllProperties()
    {
        var json = @"{
            ""product"": {
                ""name"": ""Sample Product"",
                ""description"": ""A detailed description"",
                ""price"": 99.99,
                ""quantity"": 100,
                ""isActive"": true,
                ""createdAt"": ""2024-01-15T10:30:00Z"",
                ""tags"": [""electronics"", ""sale""],
                ""metadata"": { ""brand"": ""Acme"" }
            }
        }";

        var entities = _parser.Parse(json);
        var entity = entities[0];

        entity.Properties.Should().HaveCount(9); // 8 properties + auto-added Id
        entity.Properties.Should().Contain(p => p.Name == "Id" && p.IsId);
        entity.Properties.Should().Contain(p => p.Name == "Name" && p.CSharpType == "string");
        entity.Properties.Should().Contain(p => p.Name == "Description" && p.CSharpType == "string");
        entity.Properties.Should().Contain(p => p.Name == "Price" && p.CSharpType == "decimal");
        entity.Properties.Should().Contain(p => p.Name == "Quantity" && p.CSharpType == "int");
        entity.Properties.Should().Contain(p => p.Name == "IsActive" && p.CSharpType == "bool");
        entity.Properties.Should().Contain(p => p.Name == "CreatedAt" && p.CSharpType == "DateTime");
        entity.Properties.Should().Contain(p => p.Name == "Tags" && p.IsCollection);
        entity.Properties.Should().Contain(p => p.Name == "Metadata" && p.IsObject);
    }

    #endregion
}
