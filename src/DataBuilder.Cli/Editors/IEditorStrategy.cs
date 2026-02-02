using System.Diagnostics;

namespace DataBuilder.Cli.Editors;

public interface IEditorStrategy
{
    string Name { get; }
    int Priority { get; }
    bool IsAvailable();
    string GetCommand();
}

public class VsCodeEditorStrategy : IEditorStrategy
{
    public string Name => "Visual Studio Code";
    public int Priority => 100;

    public bool IsAvailable()
    {
        // Check if code command is available
        return TryFindExecutable("code");
    }

    public string GetCommand() => "code --wait";

    private static bool TryFindExecutable(string name)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                Arguments = name,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

public class EnvironmentEditorStrategy : IEditorStrategy
{
    public string Name => "Environment Editor";
    public int Priority => 50;

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("EDITOR")) ||
               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VISUAL"));
    }

    public string GetCommand()
    {
        return Environment.GetEnvironmentVariable("EDITOR")
            ?? Environment.GetEnvironmentVariable("VISUAL")
            ?? throw new InvalidOperationException("No editor configured");
    }
}

public class FallbackEditorStrategy : IEditorStrategy
{
    public string Name => "Fallback Editor";
    public int Priority => 0;

    public bool IsAvailable() => true;

    public string GetCommand()
    {
        if (OperatingSystem.IsWindows()) return "notepad";
        if (OperatingSystem.IsMacOS()) return "nano";
        return "nano";
    }
}

public class EditorSelector
{
    private readonly IEnumerable<IEditorStrategy> _strategies;

    public EditorSelector(IEnumerable<IEditorStrategy> strategies)
    {
        _strategies = strategies.OrderByDescending(s => s.Priority);
    }

    public IEditorStrategy Select()
    {
        return _strategies.First(s => s.IsAvailable());
    }
}
