using System.Diagnostics;

namespace DataBuilder.ModelAdd.Console;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var solutionName = "model-add-demo";
        var outputDirectory = @"C:\projects\DataBuilder\artifacts";
        var initialEntitiesFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "initial-entities.json");
        var newEntityFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "new-entity.json");

        // Ensure paths are absolute
        initialEntitiesFile = Path.GetFullPath(initialEntitiesFile);
        newEntityFile = Path.GetFullPath(newEntityFile);

        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("  DataBuilder Model-Add Command Demo");
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine();
        System.Console.WriteLine($"Solution Name: {solutionName}");
        System.Console.WriteLine($"Output Directory: {outputDirectory}");
        System.Console.WriteLine($"Initial Entities: {initialEntitiesFile}");
        System.Console.WriteLine($"New Entity: {newEntityFile}");
        System.Console.WriteLine();

        // Verify files exist
        if (!File.Exists(initialEntitiesFile))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: Initial entities file not found: {initialEntitiesFile}");
            System.Console.ResetColor();
            return 1;
        }

        if (!File.Exists(newEntityFile))
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Error: New entity file not found: {newEntityFile}");
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

        // STEP 1: Create initial solution with one entity
        System.Console.WriteLine();
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("  STEP 1: Create initial solution");
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine();

        var exitCode = await RunDbToolAsync("solution-create",
            $"--name {solutionName} --directory \"{outputDirectory}\" --json-file \"{initialEntitiesFile}\"");

        if (exitCode != 0)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Solution creation failed with exit code: {exitCode}");
            System.Console.ResetColor();
            return exitCode;
        }

        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("Initial solution created successfully!");
        System.Console.ResetColor();

        // STEP 2: Add a new model to the existing solution
        System.Console.WriteLine();
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("  STEP 2: Add new model using model-add");
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine();

        // Change working directory to the solution directory for model-add
        var solutionDir = Path.Combine(outputDirectory, ToPascalCase(solutionName));

        exitCode = await RunDbToolAsync("model-add",
            $"--json-file \"{newEntityFile}\"",
            workingDirectory: solutionDir);

        if (exitCode != 0)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Model-add failed with exit code: {exitCode}");
            System.Console.ResetColor();
            return exitCode;
        }

        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("New model added successfully!");
        System.Console.ResetColor();

        // STEP 3: Build the solution to verify
        System.Console.WriteLine();
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("  STEP 3: Build solution to verify");
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine();

        exitCode = await RunDotnetBuildAsync(solutionDir);

        if (exitCode != 0)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"Build completed with warnings or errors (exit code: {exitCode})");
            System.Console.ResetColor();
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Solution builds successfully!");
            System.Console.ResetColor();
        }

        // Summary
        System.Console.WriteLine();
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine("  Summary");
        System.Console.WriteLine("===========================================");
        System.Console.WriteLine();
        System.Console.WriteLine($"Solution Location: {targetDir}");
        System.Console.WriteLine();
        System.Console.WriteLine("Initial entity (from solution-create):");
        System.Console.WriteLine("  - Product");
        System.Console.WriteLine();
        System.Console.WriteLine("Added entity (via model-add):");
        System.Console.WriteLine("  - Order");
        System.Console.WriteLine();
        System.Console.WriteLine("Generated files for Order:");
        System.Console.WriteLine($"  - {ToPascalCase(solutionName)}.Core/Models/Order.cs");
        System.Console.WriteLine($"  - {ToPascalCase(solutionName)}.Infrastructure/Data/OrderRepository.cs");
        System.Console.WriteLine($"  - {ToPascalCase(solutionName)}.Api/Controllers/OrdersController.cs");
        System.Console.WriteLine($"  - {ToPascalCase(solutionName)}.Ui/src/app/models/order.model.ts");
        System.Console.WriteLine($"  - {ToPascalCase(solutionName)}.Ui/src/app/services/order.service.ts");
        System.Console.WriteLine($"  - {ToPascalCase(solutionName)}.Ui/src/app/features/order/");
        System.Console.WriteLine();

        return 0;
    }

    static async Task<int> RunDbToolAsync(string command, string arguments, string? workingDirectory = null)
    {
        // Use the local build of the db tool
        var dbToolPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "src", "DataBuilder.Cli", "bin", "Debug", "net9.0", "db.exe"));

        System.Console.WriteLine($"Executing: db {command} {arguments}");
        System.Console.WriteLine("-------------------------------------------");
        System.Console.WriteLine();

        var startInfo = new ProcessStartInfo
        {
            FileName = dbToolPath,
            Arguments = $"{command} {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
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

        System.Console.WriteLine();
        System.Console.WriteLine("-------------------------------------------");

        return process.ExitCode;
    }

    static async Task<int> RunDotnetBuildAsync(string solutionDir)
    {
        var apiProject = Path.Combine(solutionDir, "src", $"{Path.GetFileName(solutionDir)}.Api", $"{Path.GetFileName(solutionDir)}.Api.csproj");

        System.Console.WriteLine($"Building: {apiProject}");
        System.Console.WriteLine("-------------------------------------------");
        System.Console.WriteLine();

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{apiProject}\" --verbosity minimal",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = solutionDir
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

        System.Console.WriteLine();
        System.Console.WriteLine("-------------------------------------------");

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
