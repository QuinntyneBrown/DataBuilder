using System.CommandLine;
using DataBuilder.Cli.Models;
using DataBuilder.Cli.Services;
using DataBuilder.Cli.Generators.Api;
using DataBuilder.Cli.Generators.Angular;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli.Commands;

/// <summary>
/// Command for adding a new model to an existing solution.
/// </summary>
public class ModelAddCommand : Command
{
    public Option<FileInfo?> JsonFileOption { get; }
    public Option<bool> UseTypeDiscriminatorOption { get; }
    public Option<string> BucketOption { get; }
    public Option<string> ScopeOption { get; }
    public Option<string?> CollectionOption { get; }

    public ModelAddCommand() : base("model-add", "Add a new model with CRUD operations to an existing solution")
    {
        JsonFileOption = new Option<FileInfo?>("--json-file");
        JsonFileOption.Description = "Path to a JSON file defining the entity (skips interactive editor)";
        JsonFileOption.Aliases.Add("-j");

        UseTypeDiscriminatorOption = new Option<bool>("--use-type-discriminator");
        UseTypeDiscriminatorOption.Description = "Use a type discriminator field instead of separate collections per entity (default: false)";
        UseTypeDiscriminatorOption.DefaultValueFactory = (_) => false;

        BucketOption = new Option<string>("--bucket");
        BucketOption.Description = "The Couchbase bucket name (default: general)";
        BucketOption.Aliases.Add("-b");
        BucketOption.DefaultValueFactory = (_) => "general";

        ScopeOption = new Option<string>("--scope");
        ScopeOption.Description = "The Couchbase scope name (default: general)";
        ScopeOption.Aliases.Add("-s");
        ScopeOption.DefaultValueFactory = (_) => "general";

        CollectionOption = new Option<string?>("--collection");
        CollectionOption.Description = "The Couchbase collection name (default: entity name when not using type discriminator, 'general' otherwise)";
        CollectionOption.Aliases.Add("-c");

        Options.Add(JsonFileOption);
        Options.Add(UseTypeDiscriminatorOption);
        Options.Add(BucketOption);
        Options.Add(ScopeOption);
        Options.Add(CollectionOption);
    }
}

/// <summary>
/// Handler for the model-add command.
/// </summary>
public class ModelAddCommandHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ModelAddCommandHandler> _logger;

    public ModelAddCommandHandler(IServiceProvider serviceProvider, ILogger<ModelAddCommandHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<int> HandleAsync(FileInfo? jsonFile, bool useTypeDiscriminator, string bucket, string scope, string? collection, CancellationToken cancellationToken)
    {
        try
        {
            // Find the solution file in current directory or parent directories
            var solutionPath = FindSolutionFile(Directory.GetCurrentDirectory());
            if (solutionPath == null)
            {
                _logger.LogError("No solution file (.sln) found in current directory or parent directories.");
                Console.Error.WriteLine("Error: No solution file found. Please run this command from within a solution directory.");
                return 1;
            }

            _logger.LogInformation("Found solution: {Solution}", Path.GetFileName(solutionPath));

            // Validate solution structure
            var solutionDirectory = Path.GetDirectoryName(solutionPath)!;
            var validationResult = ValidateSolutionStructure(solutionDirectory);
            if (!validationResult.IsValid)
            {
                _logger.LogError("Invalid solution structure: {Error}", validationResult.ErrorMessage);
                Console.Error.WriteLine($"Error: {validationResult.ErrorMessage}");
                return 1;
            }

            var solutionOptions = validationResult.SolutionOptions!;
            _logger.LogInformation("Validated solution structure for: {Name}", solutionOptions.Name);

            // Get entity JSON from file or editor
            string json;
            if (jsonFile != null)
            {
                if (!jsonFile.Exists)
                {
                    _logger.LogError("JSON file not found: {Path}", jsonFile.FullName);
                    return 1;
                }
                json = await File.ReadAllTextAsync(jsonFile.FullName, cancellationToken);
                _logger.LogInformation("Read entity definition from: {Path}", jsonFile.FullName);
            }
            else
            {
                var jsonEditorService = _serviceProvider.GetRequiredService<IJsonEditorService>();

                var sampleJson = @"{
  ""product"": {
    ""name"": ""Sample Product"",
    ""description"": ""Sample description"",
    ""price"": 99.99,
    ""inStock"": true,
    ""releaseDate"": ""2024-12-31""
  }
}";

                _logger.LogInformation("Opening JSON editor for entity definition...");
                Console.WriteLine();
                Console.WriteLine("Define your entity in JSON format.");
                Console.WriteLine("The top-level property becomes the entity name, its value defines the entity's properties.");
                Console.WriteLine("Save and close the editor when done.");
                Console.WriteLine();

                json = await jsonEditorService.EditJsonAsync(sampleJson, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogError("No JSON provided. Aborting.");
                return 1;
            }

            // Parse the JSON to get entity definition
            var schemaParser = _serviceProvider.GetRequiredService<ISchemaParser>();
            var entities = schemaParser.Parse(json);

            if (entities.Count == 0)
            {
                _logger.LogError("No entities found in JSON. Aborting.");
                return 1;
            }

            if (entities.Count > 1)
            {
                _logger.LogWarning("Multiple entities found in JSON. Only the first entity will be added.");
            }

            var entity = entities[0];

            // Apply Couchbase settings to the entity
            // Only override UseTypeDiscriminator if CLI option is true
            // Otherwise preserve auto-detected value from JSON "type" property
            if (useTypeDiscriminator)
            {
                entity.UseTypeDiscriminator = true;
            }
            entity.Bucket = bucket;
            entity.Scope = scope;
            if (collection != null)
            {
                entity.CollectionOverride = collection;
            }

            _logger.LogInformation("Adding entity: {Name}", entity.Name);

            // Check if entity already exists
            var entityFilePath = Path.Combine(solutionOptions.CoreProjectDirectory, "Models", $"{entity.Name}.cs");
            if (File.Exists(entityFilePath))
            {
                _logger.LogError("Entity {Name} already exists at {Path}", entity.Name, entityFilePath);
                Console.Error.WriteLine($"Error: Entity '{entity.Name}' already exists in the solution.");
                return 1;
            }

            // Add the entity to the solution
            var modelAddService = _serviceProvider.GetRequiredService<IModelAddService>();
            await modelAddService.AddModelAsync(solutionOptions, entity, cancellationToken);

            Console.WriteLine();
            Console.WriteLine($"Model '{entity.Name}' added successfully!");
            Console.WriteLine();
            Console.WriteLine("Generated files:");
            Console.WriteLine($"  - {solutionOptions.CoreProjectName}/Models/{entity.Name}.cs");
            Console.WriteLine($"  - {solutionOptions.InfrastructureProjectName}/Data/{entity.Name}Repository.cs");
            Console.WriteLine($"  - {solutionOptions.ApiProjectName}/Controllers/{entity.NamePlural}Controller.cs");
            Console.WriteLine($"  - {solutionOptions.UiProjectName}/src/app/models/{entity.NameKebabCase}.model.ts");
            Console.WriteLine($"  - {solutionOptions.UiProjectName}/src/app/services/{entity.NameKebabCase}.service.ts");
            Console.WriteLine($"  - {solutionOptions.UiProjectName}/src/app/features/{entity.NameKebabCase}/ (components)");
            Console.WriteLine();
            Console.WriteLine("Updated files:");
            Console.WriteLine($"  - {solutionOptions.InfrastructureProjectName}/ServiceCollectionExtensions.cs");
            Console.WriteLine($"  - {solutionOptions.UiProjectName}/src/app/app.routes.ts");
            Console.WriteLine($"  - {solutionOptions.UiProjectName}/src/app/app.component.ts");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine($"  1. Rebuild the solution: dotnet build");
            Console.WriteLine($"  2. Run migrations if using EF Core");
            Console.WriteLine($"  3. Test the new API endpoints and UI screens");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add model");
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private string? FindSolutionFile(string startDirectory)
    {
        var directory = startDirectory;
        while (directory != null)
        {
            var solutionFiles = Directory.GetFiles(directory, "*.sln");
            if (solutionFiles.Length > 0)
            {
                return solutionFiles[0];
            }

            directory = Directory.GetParent(directory)?.FullName;
        }
        return null;
    }

    private ValidationResult ValidateSolutionStructure(string solutionDirectory)
    {
        // Extract solution name from directory or .sln file
        var solutionFiles = Directory.GetFiles(solutionDirectory, "*.sln");
        if (solutionFiles.Length == 0)
        {
            return ValidationResult.Invalid("No solution file found.");
        }

        var solutionName = Path.GetFileNameWithoutExtension(solutionFiles[0]);
        var srcDirectory = Path.Combine(solutionDirectory, "src");

        if (!Directory.Exists(srcDirectory))
        {
            return ValidationResult.Invalid("Expected 'src' directory not found. This solution may not have been created by solution-create.");
        }

        // Check for expected project directories
        var coreProject = Path.Combine(srcDirectory, $"{solutionName}.Core");
        var infrastructureProject = Path.Combine(srcDirectory, $"{solutionName}.Infrastructure");
        var apiProject = Path.Combine(srcDirectory, $"{solutionName}.Api");
        var uiProject = Path.Combine(srcDirectory, $"{solutionName}.Ui");

        if (!Directory.Exists(coreProject))
        {
            return ValidationResult.Invalid($"Expected project directory not found: {solutionName}.Core");
        }
        if (!Directory.Exists(infrastructureProject))
        {
            return ValidationResult.Invalid($"Expected project directory not found: {solutionName}.Infrastructure");
        }
        if (!Directory.Exists(apiProject))
        {
            return ValidationResult.Invalid($"Expected project directory not found: {solutionName}.Api");
        }
        if (!Directory.Exists(uiProject))
        {
            return ValidationResult.Invalid($"Expected project directory not found: {solutionName}.Ui");
        }

        // Create SolutionOptions from discovered structure
        // Directory should be the parent of the solution directory
        var parentDirectory = Path.GetDirectoryName(solutionDirectory) ?? solutionDirectory;
        var options = new SolutionOptions
        {
            Name = solutionName,
            Directory = parentDirectory,
            Entities = new List<EntityDefinition>() // Will be populated as models are added
        };

        return ValidationResult.Valid(options);
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public SolutionOptions? SolutionOptions { get; set; }

        public static ValidationResult Valid(SolutionOptions options) =>
            new ValidationResult { IsValid = true, SolutionOptions = options };

        public static ValidationResult Invalid(string errorMessage) =>
            new ValidationResult { IsValid = false, ErrorMessage = errorMessage };
    }
}
