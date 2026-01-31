using DataBuilder.Cli.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataBuilder.Cli.Tests.Utilities;

public class ProcessResultTests
{
    [Fact]
    public void Success_WithZeroExitCode_ReturnsTrue()
    {
        var result = new ProcessResult { ExitCode = 0 };
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void Success_WithNonZeroExitCode_ReturnsFalse()
    {
        var result = new ProcessResult { ExitCode = 1 };
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void Success_WithNegativeExitCode_ReturnsFalse()
    {
        var result = new ProcessResult { ExitCode = -1 };
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void DefaultProperties_ShouldHaveCorrectDefaults()
    {
        var result = new ProcessResult();
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().BeEmpty();
        result.StandardError.Should().BeEmpty();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var result = new ProcessResult
        {
            ExitCode = 42,
            StandardOutput = "output",
            StandardError = "error"
        };

        result.ExitCode.Should().Be(42);
        result.StandardOutput.Should().Be("output");
        result.StandardError.Should().Be("error");
    }
}

public class ProcessRunnerTests
{
    private readonly Mock<ILogger<ProcessRunner>> _loggerMock;
    private readonly ProcessRunner _processRunner;

    public ProcessRunnerTests()
    {
        _loggerMock = new Mock<ILogger<ProcessRunner>>();
        _processRunner = new ProcessRunner(_loggerMock.Object);
    }

    [Fact]
    public async Task RunAsync_WithEchoCommand_ReturnsOutput()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();

        // Act
        var result = await _processRunner.RunAsync("echo", "hello", workingDirectory);

        // Assert
        result.Success.Should().BeTrue();
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("hello");
    }

    [Fact]
    public async Task RunAsync_WithInvalidCommand_ReturnsNonZeroExitCode()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();

        // Act - Use a command that doesn't exist
        var result = await _processRunner.RunAsync("nonexistentcommand12345", "", workingDirectory);

        // Assert
        result.Success.Should().BeFalse();
        result.ExitCode.Should().NotBe(0);
    }

    [Fact]
    public async Task RunWithOutputAsync_WithEchoCommand_ReturnsOutput()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();

        // Act
        var result = await _processRunner.RunWithOutputAsync("echo", "world", workingDirectory);

        // Assert
        result.Success.Should().BeTrue();
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("world");
    }

    [Fact]
    public async Task RunAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();
        using var cts = new CancellationTokenSource();

        // Act & Assert - This should complete without throwing when not cancelled
        var result = await _processRunner.RunAsync("echo", "test", workingDirectory, cts.Token);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RunWithOutputAsync_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();
        using var cts = new CancellationTokenSource();

        // Act & Assert - This should complete without throwing when not cancelled
        var result = await _processRunner.RunWithOutputAsync("echo", "test", workingDirectory, cts.Token);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_LogsDebugMessages()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();

        // Act
        await _processRunner.RunAsync("echo", "test", workingDirectory);

        // Assert - Verify logging was called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunWithOutputAsync_LogsInformationMessages()
    {
        // Arrange
        var workingDirectory = Path.GetTempPath();

        // Act
        await _processRunner.RunWithOutputAsync("echo", "test", workingDirectory);

        // Assert - Verify logging was called
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
