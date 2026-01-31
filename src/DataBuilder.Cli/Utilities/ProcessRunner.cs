using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli.Utilities;

/// <summary>
/// Runs external processes like ng CLI commands.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs a command and returns the result.
    /// </summary>
    Task<ProcessResult> RunAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a command and returns the result, with real-time output logging.
    /// </summary>
    Task<ProcessResult> RunWithOutputAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default);
}

public class ProcessResult
{
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public bool Success => ExitCode == 0;
}

public class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    public async Task<ProcessResult> RunAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Running: {Command} {Arguments} in {WorkingDirectory}", command, arguments, workingDirectory);

        var isWindows = OperatingSystem.IsWindows();
        var fileName = isWindows ? "cmd.exe" : "/bin/bash";
        var args = isWindows ? $"/c {command} {arguments}" : $"-c \"{command} {arguments}\"";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        _logger.LogDebug("Process exited with code {ExitCode}", process.ExitCode);

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = output,
            StandardError = error
        };
    }

    public async Task<ProcessResult> RunWithOutputAsync(string command, string arguments, string workingDirectory, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Running: {Command} {Arguments}", command, arguments);

        var isWindows = OperatingSystem.IsWindows();
        var fileName = isWindows ? "cmd.exe" : "/bin/bash";
        var args = isWindows ? $"/c {command} {arguments}" : $"-c \"{command} {arguments}\"";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var output = new System.Text.StringBuilder();
        var error = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                _logger.LogInformation("{Output}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
                _logger.LogWarning("{Error}", e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        _logger.LogInformation("Process exited with code {ExitCode}", process.ExitCode);

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = output.ToString(),
            StandardError = error.ToString()
        };
    }
}
