using DataBuilder.Cli.Generators.Angular;
using DataBuilder.Cli.Models;
using DataBuilder.Cli.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataBuilder.Cli.Tests.Generators;

public class AngularGeneratorTests : IDisposable
{
    private readonly Mock<IProcessRunner> _processRunnerMock;
    private readonly Mock<ILogger<AngularGenerator>> _loggerMock;
    private readonly AngularGenerator _generator;
    private readonly string _testDirectory;

    public AngularGeneratorTests()
    {
        _processRunnerMock = new Mock<IProcessRunner>();
        _loggerMock = new Mock<ILogger<AngularGenerator>>();
        _generator = new AngularGenerator(_processRunnerMock.Object, _loggerMock.Object);

        _testDirectory = Path.Combine(Path.GetTempPath(), $"DataBuilder_AngularTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);

        // Setup process runner to succeed
        _processRunnerMock
            .Setup(x => x.RunWithOutputAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessResult { ExitCode = 0, StandardOutput = "" });
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    private SolutionOptions CreateTestOptions(List<EntityDefinition>? entities = null)
    {
        return new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = entities ?? new List<EntityDefinition>
            {
                new()
                {
                    Name = "Product",
                    Properties = new List<PropertyDefinition>
                    {
                        new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                        new() { Name = "Name", NameCamelCase = "name", CSharpType = "string", TypeScriptType = "string" },
                        new() { Name = "Price", NameCamelCase = "price", CSharpType = "decimal", TypeScriptType = "number" }
                    }
                }
            }
        };
    }

    [Fact]
    public async Task GenerateModelAsync_ReturnsModelCode()
    {
        // Arrange
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                new() { Name = "Name", NameCamelCase = "name", CSharpType = "string", TypeScriptType = "string" },
                new() { Name = "Price", NameCamelCase = "price", CSharpType = "decimal", TypeScriptType = "number" }
            }
        };

        // Act
        var code = await _generator.GenerateModelAsync(entity);

        // Assert
        code.Should().Contain("export interface Product");
        code.Should().Contain("id: string");
        code.Should().Contain("name: string");
        code.Should().Contain("price: number");
    }

    [Fact]
    public async Task GenerateServiceAsync_ReturnsServiceCode()
    {
        // Arrange
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true }
            }
        };

        // Act
        var code = await _generator.GenerateServiceAsync(entity);

        // Assert
        code.Should().Contain("@Injectable");
        code.Should().Contain("export class ProductService");
        code.Should().Contain("HttpClient");
    }

    [Fact]
    public async Task GenerateListComponentAsync_ReturnsComponentContent()
    {
        // Arrange
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                new() { Name = "Name", NameCamelCase = "name", CSharpType = "string", TypeScriptType = "string" }
            }
        };

        // Act
        var content = await _generator.GenerateListComponentAsync(entity);

        // Assert
        content.Should().NotBeNull();
        content.Ts.Should().Contain("export class ProductListComponent");
    }

    [Fact]
    public async Task GenerateDetailComponentAsync_ReturnsComponentContent()
    {
        // Arrange
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                new() { Name = "Name", NameCamelCase = "name", CSharpType = "string", TypeScriptType = "string" }
            }
        };

        // Act
        var content = await _generator.GenerateDetailComponentAsync(entity);

        // Assert
        content.Should().NotBeNull();
        content.Ts.Should().Contain("export class ProductDetailComponent");
    }

    [Fact]
    public async Task GenerateModelAsync_WithNullableProperty_IncludesNullType()
    {
        // Arrange
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                new() { Name = "OptionalField", NameCamelCase = "optionalField", CSharpType = "string?", TypeScriptType = "string | null", IsNullable = true }
            }
        };

        // Act
        var code = await _generator.GenerateModelAsync(entity);

        // Assert
        code.Should().Contain("optionalField");
    }

    [Fact]
    public async Task GenerateModelAsync_WithCollectionProperty_IncludesArray()
    {
        // Arrange
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                new() { Name = "Tags", NameCamelCase = "tags", CSharpType = "List<string>", TypeScriptType = "string[]", IsCollection = true }
            }
        };

        // Act
        var code = await _generator.GenerateModelAsync(entity);

        // Assert
        code.Should().Contain("tags: string[]");
    }

    [Fact]
    public async Task GenerateServiceAsync_IncludesCrudMethods()
    {
        // Arrange
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true }
            }
        };

        // Act
        var code = await _generator.GenerateServiceAsync(entity);

        // Assert
        code.Should().Contain("getAll");
        code.Should().Contain("getById");
        code.Should().Contain("create");
        code.Should().Contain("update");
        code.Should().Contain("delete");
    }
}
