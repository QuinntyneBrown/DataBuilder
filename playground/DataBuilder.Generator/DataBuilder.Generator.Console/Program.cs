using System.Diagnostics;

namespace DataBuilder.Generator.Console;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var solutionName = args.Length > 0 ? args[0] : "idea-app";
        var outputDirectory = @"C:\projects\DataBuilder\artifacts";
        var entitiesFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "entities.json");

        // Ensure entities file path is absolute
        entitiesFile = Path.GetFullPath(entitiesFile);

        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("  DataBuilder Solution Generator");
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine();
        System.Console.WriteLine($"Solution Name: {solutionName}");
        System.Console.WriteLine($"Output Directory: {outputDirectory}");
        System.Console.WriteLine($"Entities File: {entitiesFile}");
        System.Console.WriteLine();

        // Verify entities file exists
        if (!File.Exists(entitiesFile))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: Entities file not found: {entitiesFile}");
            System.Console.ResetColor();
            return 1;
        }

        // Ensure output directory exists
        Directory.CreateDirectory(outputDirectory);

        // Clean up previous output if it exists
        var targetDir = Path.Combine(outputDirectory, ToPascalCase(solutionName));
        if (Directory.Exists(targetDir))
        {
            System.Console.WriteLine($"Removing existing directory: {targetDir}");
            await DeleteDirectoryWithRetryAsync(targetDir);
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Executing: db solution-create");
        System.Console.WriteLine("-------------------------------------------");
        System.Console.WriteLine();

        // Execute the db tool
        var exitCode = await RunDbToolAsync(solutionName, outputDirectory, entitiesFile);

        System.Console.WriteLine();
        System.Console.WriteLine("-------------------------------------------");

        if (exitCode == 0)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Solution generated successfully!");
            System.Console.ResetColor();
            System.Console.WriteLine();
            System.Console.WriteLine($"Location: {targetDir}");
            System.Console.WriteLine();
            System.Console.WriteLine("Project Structure:");
            System.Console.WriteLine($"  {ToPascalCase(solutionName)}/");
            System.Console.WriteLine($"    ├── {ToPascalCase(solutionName)}.sln");
            System.Console.WriteLine($"    └── src/");
            System.Console.WriteLine($"        ├── {ToPascalCase(solutionName)}.Core/");
            System.Console.WriteLine($"        ├── {ToPascalCase(solutionName)}.Infrastructure/");
            System.Console.WriteLine($"        ├── {ToPascalCase(solutionName)}.Api/");
            System.Console.WriteLine($"        └── {ToPascalCase(solutionName)}.Ui/");
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Solution generation failed with exit code: {exitCode}");
            System.Console.ResetColor();
        }

        return exitCode;
    }

    static async Task<int> RunDbToolAsync(string solutionName, string outputDirectory, string entitiesFile)
    {
        // Use the local build of the db tool
        var dbToolPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "src", "DataBuilder.Cli", "bin", "Debug", "net9.0", "db.exe"));

        var startInfo = new ProcessStartInfo
        {
            FileName = dbToolPath,
            Arguments = $"solution-create --name {solutionName} --directory \"{outputDirectory}\" --json-file \"{entitiesFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                System.Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine(e.Data);
                System.Console.ResetColor();
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode;
    }

    static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split(new[] { '-', '_', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var result = string.Join("", words.Select(w =>
            char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w[1..] : "")));

        return result;
    }

    static async Task DeleteDirectoryWithRetryAsync(string path, int maxRetries = 3, int delayMs = 1000)
    {
        Exception? lastException = null;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Try using cmd.exe rmdir first (handles some lock scenarios better on Windows)
                if (OperatingSystem.IsWindows())
                {
                    await RunCommandAsync("cmd.exe", $"/c rmdir /s /q \"{path}\"");
                    if (!Directory.Exists(path))
                        return;
                }

                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException ex)
            {
                lastException = ex;
                System.Console.WriteLine($"  Directory locked, retrying in {delayMs}ms... (attempt {i + 1}/{maxRetries})");
                await Task.Delay(delayMs);
            }
            catch (UnauthorizedAccessException ex)
            {
                lastException = ex;
                RemoveReadOnlyAttributes(path);
                System.Console.WriteLine($"  Access denied, removed read-only flags, retrying... (attempt {i + 1}/{maxRetries})");
                await Task.Delay(delayMs);
            }
        }

        // If we get here and directory still exists, warn but continue
        if (Directory.Exists(path))
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"  Warning: Could not fully delete {path}. Continuing anyway...");
            System.Console.ResetColor();
        }
    }

    static async Task<int> RunCommandAsync(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    static void RemoveReadOnlyAttributes(string path)
    {
        try
        {
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var attr = File.GetAttributes(file);
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
                }
            }
        }
        catch { /* Ignore errors during attribute removal */ }
    }
}
