using DataBuilder.Cli.Services;

namespace DataBuilder.Cli.Tests.TestInfrastructure;

/// <summary>
/// In-memory file system implementation for testing.
/// </summary>
public class InMemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new();
    private readonly HashSet<string> _directories = new();

    public Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
    {
        if (!_files.TryGetValue(NormalizePath(path), out var content))
            throw new FileNotFoundException("File not found", path);
        return Task.FromResult(content);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
    {
        var normalizedPath = NormalizePath(path);
        _files[normalizedPath] = content;

        // Ensure parent directory exists
        var directory = Path.GetDirectoryName(normalizedPath);
        if (!string.IsNullOrEmpty(directory))
            _directories.Add(directory);

        return Task.CompletedTask;
    }

    public bool FileExists(string path) => _files.ContainsKey(NormalizePath(path));

    public bool DirectoryExists(string path) => _directories.Contains(NormalizePath(path));

    public void CreateDirectory(string path) => _directories.Add(NormalizePath(path));

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        var normalizedPath = NormalizePath(path);
        var pattern = ConvertSearchPatternToRegex(searchPattern);

        return _files.Keys
            .Where(f =>
            {
                var dir = Path.GetDirectoryName(f) ?? "";
                var fileName = Path.GetFileName(f);

                if (searchOption == SearchOption.TopDirectoryOnly)
                    return dir.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase)
                        && System.Text.RegularExpressions.Regex.IsMatch(fileName, pattern);

                return (dir.StartsWith(normalizedPath, StringComparison.OrdinalIgnoreCase) || dir.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
                    && System.Text.RegularExpressions.Regex.IsMatch(fileName, pattern);
            })
            .ToArray();
    }

    public string[] GetDirectories(string path)
    {
        var normalizedPath = NormalizePath(path);
        return _directories
            .Where(d => Path.GetDirectoryName(d)?.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase) == true)
            .ToArray();
    }

    public void Delete(string path)
    {
        _files.Remove(NormalizePath(path));
    }

    /// <summary>
    /// Gets a read-only view of all files in the mock file system.
    /// </summary>
    public IReadOnlyDictionary<string, string> Files => _files;

    /// <summary>
    /// Gets a read-only view of all directories in the mock file system.
    /// </summary>
    public IReadOnlyCollection<string> Directories => _directories;

    /// <summary>
    /// Adds a file to the mock file system without going through WriteAllTextAsync.
    /// Useful for setting up test fixtures.
    /// </summary>
    public void AddFile(string path, string content)
    {
        _files[NormalizePath(path)] = content;
    }

    /// <summary>
    /// Clears all files and directories from the mock file system.
    /// </summary>
    public void Clear()
    {
        _files.Clear();
        _directories.Clear();
    }

    private static string NormalizePath(string path)
        => path.Replace('\\', '/').TrimEnd('/');

    private static string ConvertSearchPatternToRegex(string pattern)
    {
        var escaped = System.Text.RegularExpressions.Regex.Escape(pattern);
        return "^" + escaped.Replace("\\*", ".*").Replace("\\?", ".") + "$";
    }
}
