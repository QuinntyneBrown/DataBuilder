namespace DataBuilder.Cli.Services;

/// <summary>
/// Service for opening and editing JSON in an external editor.
/// </summary>
public interface IJsonEditorService
{
    /// <summary>
    /// Opens an external editor for the user to input JSON.
    /// Uses the EDITOR environment variable, falls back to notepad (Windows) or nano (Unix).
    /// </summary>
    /// <param name="initialContent">Initial content to display in the editor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edited JSON content.</returns>
    Task<string> EditJsonAsync(string initialContent, CancellationToken cancellationToken = default);
}
