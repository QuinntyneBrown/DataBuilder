using DataBuilder.Cli.Generators.Angular;
using DataBuilder.Cli.Generators.Api;
using DataBuilder.Cli.Models;
using DataBuilder.Cli.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataBuilder.Cli.Tests.Services;

public class SolutionGeneratorTests : IDisposable
{
    private readonly Mock<IApiGenerator> _apiGeneratorMock;
    private readonly Mock<IAngularGenerator> _angularGeneratorMock;
    private readonly Mock<ILogger<SolutionGenerator>> _loggerMock;
    private readonly SolutionGenerator _generator;
    private readonly string _testDirectory;

    public SolutionGeneratorTests()
    {
        _apiGeneratorMock = new Mock<IApiGenerator>();
        _angularGeneratorMock = new Mock<IAngularGenerator>();
        _loggerMock = new Mock<ILogger<SolutionGenerator>>();
        _generator = new SolutionGenerator(
            _apiGeneratorMock.Object,
            _angularGeneratorMock.Object,
            _loggerMock.Object);

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
    public async Task GenerateAsync_CreatesSolutionDirectory()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        Directory.Exists(options.SolutionDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesSrcDirectory()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        Directory.Exists(options.SrcDirectory).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CreatesSolutionFile()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var solutionFile = Path.Combine(options.SolutionDirectory, "TestApp.sln");
        File.Exists(solutionFile).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_SolutionFileContainsProjectReferences()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var solutionFile = Path.Combine(options.SolutionDirectory, "TestApp.sln");
        var content = await File.ReadAllTextAsync(solutionFile);

        content.Should().Contain("TestApp.Core");
        content.Should().Contain("TestApp.Infrastructure");
        content.Should().Contain("TestApp.Api");
    }

    [Fact]
    public async Task GenerateAsync_SolutionFileHasCorrectFormat()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        var solutionFile = Path.Combine(options.SolutionDirectory, "TestApp.sln");
        var content = await File.ReadAllTextAsync(solutionFile);

        content.Should().Contain("Microsoft Visual Studio Solution File");
        content.Should().Contain("GlobalSection(SolutionConfigurationPlatforms)");
        content.Should().Contain("Debug|Any CPU");
        content.Should().Contain("Release|Any CPU");
    }

    [Fact]
    public async Task GenerateAsync_CallsApiGenerator()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        _apiGeneratorMock.Verify(
            x => x.GenerateAsync(options, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_CallsAngularGenerator()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

        // Act
        await _generator.GenerateAsync(options);

        // Assert
        _angularGeneratorMock.Verify(
            x => x.GenerateAsync(options, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_LogsProgress()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };

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
            Times.AtLeast(3)); // At least 3 info logs
    }

    [Fact]
    public async Task GenerateAsync_WithCancellationToken_PassesToGenerators()
    {
        // Arrange
        var options = new SolutionOptions
        {
            Name = "test-app",
            Directory = _testDirectory,
            Entities = new List<EntityDefinition>()
        };
        using var cts = new CancellationTokenSource();

        // Act
        await _generator.GenerateAsync(options, cts.Token);

        // Assert
        _apiGeneratorMock.Verify(
            x => x.GenerateAsync(options, cts.Token),
            Times.Once);
        _angularGeneratorMock.Verify(
            x => x.GenerateAsync(options, cts.Token),
            Times.Once);
    }
}
