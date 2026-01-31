using System.CommandLine;
using DataBuilder.Cli.Commands;
using DataBuilder.Cli.Models;
using DataBuilder.Cli.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataBuilder.Cli.Tests.Commands;

public class SolutionCreateCommandTests
{
    [Fact]
    public void Constructor_SetsUpCommandCorrectly()
    {
        var command = new SolutionCreateCommand();

        command.Name.Should().Be("solution-create");
        command.Description.Should().Contain("full-stack solution");
    }

    [Fact]
    public void NameOption_IsRequired()
    {
        var command = new SolutionCreateCommand();

        command.NameOption.Required.Should().BeTrue();
        command.NameOption.Aliases.Should().Contain("-n");
    }

    [Fact]
    public void DirectoryOption_HasDefaultValue()
    {
        var command = new SolutionCreateCommand();

        command.DirectoryOption.Required.Should().BeFalse();
        command.DirectoryOption.Aliases.Should().Contain("-d");
    }

    [Fact]
    public void JsonFileOption_IsOptional()
    {
        var command = new SolutionCreateCommand();

        command.JsonFileOption.Required.Should().BeFalse();
        command.JsonFileOption.Aliases.Should().Contain("-j");
    }

    [Fact]
    public void UseTypeDiscriminatorOption_DefaultsToFalse()
    {
        var command = new SolutionCreateCommand();

        command.UseTypeDiscriminatorOption.Required.Should().BeFalse();
    }

    [Fact]
    public void BucketOption_HasAlias()
    {
        var command = new SolutionCreateCommand();

        command.BucketOption.Aliases.Should().Contain("-b");
    }

    [Fact]
    public void ScopeOption_HasAlias()
    {
        var command = new SolutionCreateCommand();

        command.ScopeOption.Aliases.Should().Contain("-s");
    }

    [Fact]
    public void CollectionOption_HasAlias()
    {
        var command = new SolutionCreateCommand();

        command.CollectionOption.Aliases.Should().Contain("-c");
    }

    [Fact]
    public void Command_HasAllOptions()
    {
        var command = new SolutionCreateCommand();

        command.Options.Should().HaveCount(7);
    }
}

public class SolutionCreateCommandHandlerTests : IDisposable
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<SolutionCreateCommandHandler>> _loggerMock;
    private readonly Mock<ISchemaParser> _schemaParserMock;
    private readonly Mock<ISolutionGenerator> _solutionGeneratorMock;
    private readonly Mock<IJsonEditorService> _jsonEditorServiceMock;
    private readonly SolutionCreateCommandHandler _handler;
    private readonly string _testDirectory;

    public SolutionCreateCommandHandlerTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<SolutionCreateCommandHandler>>();
        _schemaParserMock = new Mock<ISchemaParser>();
        _solutionGeneratorMock = new Mock<ISolutionGenerator>();
        _jsonEditorServiceMock = new Mock<IJsonEditorService>();

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(ISchemaParser)))
            .Returns(_schemaParserMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(ISolutionGenerator)))
            .Returns(_solutionGeneratorMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IJsonEditorService)))
            .Returns(_jsonEditorServiceMock.Object);

        _handler = new SolutionCreateCommandHandler(_serviceProviderMock.Object, _loggerMock.Object);

        _testDirectory = Path.Combine(Path.GetTempPath(), $"DataBuilder_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task HandleAsync_WithValidJsonFile_ReturnsZero()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "entities.json");
        await File.WriteAllTextAsync(jsonFile, @"{ ""product"": { ""name"": """" } }");

        var entities = new List<EntityDefinition>
        {
            new() { Name = "Product" }
        };
        _schemaParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(entities);

        // Act
        var result = await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            false,
            "general",
            "general",
            null,
            CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_WithMissingJsonFile_ReturnsOne()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "nonexistent.json");

        // Act
        var result = await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            false,
            "general",
            "general",
            null,
            CancellationToken.None);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WithNoEntities_ReturnsOne()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "entities.json");
        await File.WriteAllTextAsync(jsonFile, "{}");

        _schemaParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(new List<EntityDefinition>());

        // Act
        var result = await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            false,
            "general",
            "general",
            null,
            CancellationToken.None);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_CallsSolutionGenerator()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "entities.json");
        await File.WriteAllTextAsync(jsonFile, @"{ ""product"": { ""name"": """" } }");

        var entities = new List<EntityDefinition>
        {
            new() { Name = "Product" }
        };
        _schemaParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(entities);

        // Act
        await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            false,
            "general",
            "general",
            null,
            CancellationToken.None);

        // Assert
        _solutionGeneratorMock.Verify(
            x => x.GenerateAsync(It.IsAny<SolutionOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUseTypeDiscriminator_SetsEntityProperty()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "entities.json");
        await File.WriteAllTextAsync(jsonFile, @"{ ""product"": { ""name"": """" } }");

        var entities = new List<EntityDefinition>
        {
            new() { Name = "Product" }
        };
        _schemaParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(entities);

        SolutionOptions? capturedOptions = null;
        _solutionGeneratorMock
            .Setup(x => x.GenerateAsync(It.IsAny<SolutionOptions>(), It.IsAny<CancellationToken>()))
            .Callback<SolutionOptions, CancellationToken>((o, _) => capturedOptions = o);

        // Act
        await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            true, // useTypeDiscriminator
            "general",
            "general",
            null,
            CancellationToken.None);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.Entities.Should().AllSatisfy(e => e.UseTypeDiscriminator.Should().BeTrue());
    }

    [Fact]
    public async Task HandleAsync_WithCustomBucket_SetsEntityProperty()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "entities.json");
        await File.WriteAllTextAsync(jsonFile, @"{ ""product"": { ""name"": """" } }");

        var entities = new List<EntityDefinition>
        {
            new() { Name = "Product" }
        };
        _schemaParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(entities);

        SolutionOptions? capturedOptions = null;
        _solutionGeneratorMock
            .Setup(x => x.GenerateAsync(It.IsAny<SolutionOptions>(), It.IsAny<CancellationToken>()))
            .Callback<SolutionOptions, CancellationToken>((o, _) => capturedOptions = o);

        // Act
        await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            false,
            "mybucket",
            "myscope",
            null,
            CancellationToken.None);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.Entities.Should().AllSatisfy(e =>
        {
            e.Bucket.Should().Be("mybucket");
            e.Scope.Should().Be("myscope");
        });
    }

    [Fact]
    public async Task HandleAsync_WithCustomCollection_SetsCollectionOverride()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "entities.json");
        await File.WriteAllTextAsync(jsonFile, @"{ ""product"": { ""name"": """" } }");

        var entities = new List<EntityDefinition>
        {
            new() { Name = "Product" }
        };
        _schemaParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(entities);

        SolutionOptions? capturedOptions = null;
        _solutionGeneratorMock
            .Setup(x => x.GenerateAsync(It.IsAny<SolutionOptions>(), It.IsAny<CancellationToken>()))
            .Callback<SolutionOptions, CancellationToken>((o, _) => capturedOptions = o);

        // Act
        await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            false,
            "general",
            "general",
            "custom_collection",
            CancellationToken.None);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.Entities.Should().AllSatisfy(e =>
            e.CollectionOverride.Should().Be("custom_collection"));
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ReturnsOne()
    {
        // Arrange
        var jsonFile = Path.Combine(_testDirectory, "entities.json");
        await File.WriteAllTextAsync(jsonFile, @"{ ""product"": { ""name"": """" } }");

        _schemaParserMock
            .Setup(x => x.Parse(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Test exception"));

        // Act
        var result = await _handler.HandleAsync(
            "test-app",
            _testDirectory,
            new FileInfo(jsonFile),
            false,
            "general",
            "general",
            null,
            CancellationToken.None);

        // Assert
        result.Should().Be(1);
    }
}
