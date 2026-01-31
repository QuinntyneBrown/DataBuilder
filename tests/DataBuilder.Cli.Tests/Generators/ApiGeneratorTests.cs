using DataBuilder.Cli.Generators.Api;
using DataBuilder.Cli.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataBuilder.Cli.Tests.Generators;

public class ApiGeneratorTests : IDisposable
{
    private readonly Mock<ILogger<ApiGenerator>> _loggerMock;
    private readonly ApiGenerator _generator;
    private readonly string _testDirectory;

    public ApiGeneratorTests()
    {
        _loggerMock = new Mock<ILogger<ApiGenerator>>();
        _generator = new ApiGenerator(_loggerMock.Object);

        _testDirectory = Path.Combine(Path.GetTempPath(), $"DataBuilder_ApiTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
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
    public async Task GenerateAsync_CreatesCoreProjectDirectory()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        Directory.Exists(options.CoreProjectDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesInfrastructureProjectDirectory()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        Directory.Exists(options.InfrastructureProjectDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesApiProjectDirectory()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        Directory.Exists(options.ApiProjectDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesCoreProjectFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var csprojPath = Path.Combine(options.CoreProjectDirectory, $"{options.CoreProjectName}.csproj");
        File.Exists(csprojPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesInfrastructureProjectFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var csprojPath = Path.Combine(options.InfrastructureProjectDirectory, $"{options.InfrastructureProjectName}.csproj");
        File.Exists(csprojPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesApiProjectFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var csprojPath = Path.Combine(options.ApiProjectDirectory, $"{options.ApiProjectName}.csproj");
        File.Exists(csprojPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesEntityFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var entityPath = Path.Combine(options.CoreProjectDirectory, "Models", "Product.cs");
        File.Exists(entityPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesRepositoryFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var repoPath = Path.Combine(options.InfrastructureProjectDirectory, "Data", "ProductRepository.cs");
        File.Exists(repoPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesControllerFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var controllerPath = Path.Combine(options.ApiProjectDirectory, "Controllers", "ProductsController.cs");
        File.Exists(controllerPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesAppSettingsFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var appSettingsPath = Path.Combine(options.ApiProjectDirectory, "appsettings.json");
        File.Exists(appSettingsPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesProgramFile()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var programPath = Path.Combine(options.ApiProjectDirectory, "Program.cs");
        File.Exists(programPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesServiceCollectionExtensions()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var extensionsPath = Path.Combine(options.InfrastructureProjectDirectory, "ServiceCollectionExtensions.cs");
        File.Exists(extensionsPath).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_EntityFileContainsCorrectNamespace()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var entityPath = Path.Combine(options.CoreProjectDirectory, "Models", "Product.cs");
        var content = await File.ReadAllTextAsync(entityPath);
        content.Should().Contain($"namespace {options.CoreProjectName}.Models");
    }

    [Fact]
    public async Task GenerateAsync_EntityFileContainsProperties()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var entityPath = Path.Combine(options.CoreProjectDirectory, "Models", "Product.cs");
        var content = await File.ReadAllTextAsync(entityPath);
        content.Should().Contain("public string Id");
        content.Should().Contain("public string Name");
        content.Should().Contain("public decimal Price");
    }

    [Fact]
    public async Task GenerateAsync_WithMultipleEntities_CreatesAllFiles()
    {
        // Arrange
        var entities = new List<EntityDefinition>
        {
            new()
            {
                Name = "Product",
                Properties = new List<PropertyDefinition>
                {
                    new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                    new() { Name = "Name", NameCamelCase = "name", CSharpType = "string", TypeScriptType = "string" }
                }
            },
            new()
            {
                Name = "Category",
                Properties = new List<PropertyDefinition>
                {
                    new() { Name = "Id", NameCamelCase = "id", CSharpType = "string", TypeScriptType = "string", IsId = true },
                    new() { Name = "Title", NameCamelCase = "title", CSharpType = "string", TypeScriptType = "string" }
                }
            }
        };
        var options = CreateTestOptions(entities);

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        File.Exists(Path.Combine(options.CoreProjectDirectory, "Models", "Product.cs")).Should().BeTrue();
        File.Exists(Path.Combine(options.CoreProjectDirectory, "Models", "Category.cs")).Should().BeTrue();
        File.Exists(Path.Combine(options.InfrastructureProjectDirectory, "Data", "ProductRepository.cs")).Should().BeTrue();
        File.Exists(Path.Combine(options.InfrastructureProjectDirectory, "Data", "CategoryRepository.cs")).Should().BeTrue();
        File.Exists(Path.Combine(options.ApiProjectDirectory, "Controllers", "ProductsController.cs")).Should().BeTrue();
        File.Exists(Path.Combine(options.ApiProjectDirectory, "Controllers", "CategoriesController.cs")).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateEntityAsync_ReturnsEntityCode()
    {
        // Arrange
        var options = CreateTestOptions();
        var entity = options.Entities[0];

        // Act
        var code = await _generator.GenerateEntityAsync(options, entity);

        // Assert
        code.Should().Contain("public class Product");
        code.Should().Contain("public string Id");
    }

    [Fact]
    public async Task GenerateRepositoryAsync_ReturnsRepositoryCode()
    {
        // Arrange
        var options = CreateTestOptions();
        var entity = options.Entities[0];

        // Act
        var code = await _generator.GenerateRepositoryAsync(options, entity);

        // Assert
        code.Should().Contain("public class ProductRepository");
        code.Should().Contain("IProductRepository");
    }

    [Fact]
    public async Task GenerateControllerAsync_ReturnsControllerCode()
    {
        // Arrange
        var options = CreateTestOptions();
        var entity = options.Entities[0];

        // Act
        var code = await _generator.GenerateControllerAsync(options, entity);

        // Assert
        code.Should().Contain("public class ProductsController");
        code.Should().Contain("ControllerBase");
    }

    [Fact]
    public async Task GenerateAsync_LogsProgress()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
