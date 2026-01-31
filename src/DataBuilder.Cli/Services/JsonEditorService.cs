using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli.Services;

/// <summary>
/// Service for opening and editing JSON in an external editor.
/// </summary>
public class JsonEditorService : IJsonEditorService
{
    private readonly ILogger<JsonEditorService> _logger;

    public JsonEditorService(ILogger<JsonEditorService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> EditJsonAsync(string initialContent, CancellationToken cancellationToken = default)
    {
        // Create a temporary file with .json extension
        var tempFile = Path.Combine(Path.GetTempPath(), $"databuilder_{Guid.NewGuid():N}.json");

        try
        {
            // Write initial content to temp file
            await File.WriteAllTextAsync(tempFile, initialContent, cancellationToken);
            _logger.LogDebug("Created temp file: {TempFile}", tempFile);

            // Get the editor to use
            var editor = GetEditor();
            _logger.LogInformation("Opening editor: {Editor}", editor);

            // Open the editor
            var processInfo = new ProcessStartInfo
            {
                FileName = editor,
                Arguments = $"\"{tempFile}\"",
                UseShellExecute = true,
                CreateNoWindow = false
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new InvalidOperationException($"Failed to start editor: {editor}");
            }

            await process.WaitForExitAsync(cancellationToken);

            // Read the edited content
            var editedContent = await File.ReadAllTextAsync(tempFile, cancellationToken);
            _logger.LogDebug("Read edited content from temp file");

            return editedContent;
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempFile))
            {
                try
                {
                    File.Delete(tempFile);
                    _logger.LogDebug("Deleted temp file: {TempFile}", tempFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {TempFile}", tempFile);
                }
            }
        }
    }

    private static string GetEditor()
    {
        // Check EDITOR environment variable first
        var editor = Environment.GetEnvironmentVariable("EDITOR");
        if (!string.IsNullOrWhiteSpace(editor))
        {
            return editor;
        }

        // Check VISUAL environment variable
        editor = Environment.GetEnvironmentVariable("VISUAL");
        if (!string.IsNullOrWhiteSpace(editor))
        {
            return editor;
        }

        // Fall back to platform defaults
        if (OperatingSystem.IsWindows())
        {
            return "notepad";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "nano";
        }

        // Linux/Unix
        return "nano";
    }
}
