using System.CommandLine;
using DataBuilder.Cli.Models;
using DataBuilder.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli.Commands;

/// <summary>
/// Command for creating a new full-stack solution.
/// </summary>
public class SolutionCreateCommand : Command
{
    public Option<string> NameOption { get; }
    public Option<string> DirectoryOption { get; }
    public Option<FileInfo?> JsonFileOption { get; }

    public SolutionCreateCommand() : base("solution-create", "Create a new full-stack solution with C# API and Angular frontend")
    {
        NameOption = new Option<string>("--name");
        NameOption.Description = "The name of the solution";
        NameOption.Aliases.Add("-n");
        NameOption.Required = true;

        DirectoryOption = new Option<string>("--directory");
        DirectoryOption.Description = "The target directory for the solution";
        DirectoryOption.Aliases.Add("-d");
        DirectoryOption.DefaultValueFactory = (_) => Directory.GetCurrentDirectory();

        JsonFileOption = new Option<FileInfo?>("--json-file");
        JsonFileOption.Description = "Path to a JSON file defining entities (skips interactive editor)";
        JsonFileOption.Aliases.Add("-j");

        Options.Add(NameOption);
        Options.Add(DirectoryOption);
        Options.Add(JsonFileOption);
    }
}

/// <summary>
/// Handler for the solution-create command.
/// </summary>
public class SolutionCreateCommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SolutionCreateCommandHandler> _logger;

    public SolutionCreateCommandHandler(IServiceProvider serviceProvider, ILogger<SolutionCreateCommandHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<int> HandleAsync(string name, string directory, FileInfo? jsonFile, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating solution: {Name} in {Directory}", name, directory);

            string json;

            if (jsonFile != null)
            {
                // Read JSON from file
                if (!jsonFile.Exists)
                {
                    _logger.LogError("JSON file not found: {Path}", jsonFile.FullName);
                    return 1;
                }
                json = await File.ReadAllTextAsync(jsonFile.FullName, cancellationToken);
                _logger.LogInformation("Read entity definitions from: {Path}", jsonFile.FullName);
            }
            else
            {
                // Get the JSON editor service
                var jsonEditorService = _serviceProvider.GetRequiredService<IJsonEditorService>();

                // Create sample JSON for the user to edit
                var sampleJson = @"{
  ""toDo"": {
    ""title"": ""Sample title"",
    ""description"": ""Sample description"",
    ""isComplete"": false,
    ""priority"": 1,
    ""dueDate"": ""2024-12-31""
  }
}";

                _logger.LogInformation("Opening JSON editor for entity definition...");
                Console.WriteLine();
                Console.WriteLine("Define your entities in JSON format.");
                Console.WriteLine("Each top-level property becomes an entity, its value defines the entity's properties.");
                Console.WriteLine("Save and close the editor when done.");
                Console.WriteLine();

                json = await jsonEditorService.EditJsonAsync(sampleJson, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogError("No JSON provided. Aborting.");
                return 1;
            }

            // Parse the JSON to get entity definitions
            var schemaParser = _serviceProvider.GetRequiredService<ISchemaParser>();
            var entities = schemaParser.Parse(json);

            if (entities.Count == 0)
            {
                _logger.LogError("No entities found in JSON. Aborting.");
                return 1;
            }

            _logger.LogInformation("Found {Count} entities: {Names}",
                entities.Count, string.Join(", ", entities.Select(e => e.Name)));

            // Create solution options
            var options = new SolutionOptions
            {
                Name = name,
                Directory = directory,
                Entities = entities
            };

            // Generate the solution
            var solutionGenerator = _serviceProvider.GetRequiredService<ISolutionGenerator>();
            await solutionGenerator.GenerateAsync(options, cancellationToken);

            Console.WriteLine();
            Console.WriteLine($"Solution created successfully at: {options.SolutionDirectory}");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine($"  1. cd {options.SolutionDirectory}");
            Console.WriteLine($"  2. dotnet build {options.ApiProjectName}");
            Console.WriteLine($"  3. cd {options.UiProjectName} && npm install && ng serve");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create solution");
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
