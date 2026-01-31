using System.Reflection;
using DataBuilder.Cli.Models;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;

namespace DataBuilder.Cli.Generators.Api;

/// <summary>
/// Generates the C# N-Tier API projects (Core, Infrastructure, Api).
/// </summary>
public class ApiGenerator : IApiGenerator
{
    private readonly ILogger<ApiGenerator> _logger;
    private readonly Dictionary<string, Template> _templates = new();

    public ApiGenerator(ILogger<ApiGenerator> logger)
    {
        _logger = logger;
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templateNames = new[]
        {
            "Entity", "Controller", "Repository", "Request", "PagedResult",
            "ServiceCollectionExtensions", "Program", "appsettings",
            "csproj", "Core.csproj", "Infrastructure.csproj"
        };

        foreach (var name in templateNames)
        {
            var resourceName = $"DataBuilder.Cli.Templates.Api.{name}.sbn";
            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                _logger.LogWarning("Template not found: {ResourceName}", resourceName);
                continue;
            }

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            _templates[name] = Template.Parse(content);

            if (_templates[name].HasErrors)
            {
                _logger.LogError("Template parse errors in {Name}: {Errors}",
                    name, string.Join(", ", _templates[name].Messages));
            }
        }
    }

    /// <inheritdoc />
    public async Task GenerateAsync(SolutionOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating API projects for: {SolutionName}", options.NamePascalCase);

        // Generate Core project
        await GenerateCoreProjectAsync(options, cancellationToken);

        // Generate Infrastructure project
        await GenerateInfrastructureProjectAsync(options, cancellationToken);

        // Generate Api project
        await GenerateApiProjectAsync(options, cancellationToken);

        _logger.LogInformation("API projects generation complete");
    }

    private async Task GenerateCoreProjectAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Core project: {ProjectName}", options.CoreProjectName);

        var projectDir = options.CoreProjectDirectory;
        var modelsDir = Path.Combine(projectDir, "Models");

        Directory.CreateDirectory(modelsDir);

        // Generate project file
        await GenerateFileAsync("Core.csproj", Path.Combine(projectDir, $"{options.CoreProjectName}.csproj"),
            new { }, cancellationToken);

        // Generate entity files
        foreach (var entity in options.Entities)
        {
            var entityModel = new
            {
                Namespace = options.CoreProjectName,
                Entity = entity
            };

            await GenerateFileAsync("Entity", Path.Combine(modelsDir, $"{entity.Name}.cs"),
                entityModel, cancellationToken);

            await GenerateFileAsync("Request", Path.Combine(modelsDir, $"{entity.Name}Requests.cs"),
                entityModel, cancellationToken);
        }
    }

    private async Task GenerateInfrastructureProjectAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Infrastructure project: {ProjectName}", options.InfrastructureProjectName);

        var projectDir = options.InfrastructureProjectDirectory;
        var dataDir = Path.Combine(projectDir, "Data");

        Directory.CreateDirectory(dataDir);

        // Generate project file
        await GenerateFileAsync("Infrastructure.csproj", Path.Combine(projectDir, $"{options.InfrastructureProjectName}.csproj"),
            new { CoreProjectName = options.CoreProjectName }, cancellationToken);

        // Generate PagedResult
        await GenerateFileAsync("PagedResult", Path.Combine(dataDir, "PagedResult.cs"),
            new { Namespace = options.InfrastructureProjectName }, cancellationToken);

        // Generate repository files
        foreach (var entity in options.Entities)
        {
            var repoModel = new
            {
                Namespace = options.InfrastructureProjectName,
                CoreNamespace = options.CoreProjectName,
                Entity = entity,
                BucketName = options.CouchbaseBucket
            };

            await GenerateFileAsync("Repository", Path.Combine(dataDir, $"{entity.Name}Repository.cs"),
                repoModel, cancellationToken);
        }

        // Generate ServiceCollectionExtensions
        await GenerateFileAsync("ServiceCollectionExtensions",
            Path.Combine(projectDir, "ServiceCollectionExtensions.cs"),
            new
            {
                Namespace = options.InfrastructureProjectName,
                SolutionName = options.NamePascalCase,
                Entities = options.Entities
            }, cancellationToken);
    }

    private async Task GenerateApiProjectAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating Api project: {ProjectName}", options.ApiProjectName);

        var projectDir = options.ApiProjectDirectory;
        var controllersDir = Path.Combine(projectDir, "Controllers");

        Directory.CreateDirectory(controllersDir);

        // Generate project file
        await GenerateFileAsync("csproj", Path.Combine(projectDir, $"{options.ApiProjectName}.csproj"),
            new
            {
                CoreProjectName = options.CoreProjectName,
                InfrastructureProjectName = options.InfrastructureProjectName
            }, cancellationToken);

        // Generate Program.cs
        await GenerateFileAsync("Program", Path.Combine(projectDir, "Program.cs"),
            new
            {
                InfrastructureNamespace = options.InfrastructureProjectName,
                SolutionName = options.NamePascalCase,
                AngularPort = options.AngularPort
            }, cancellationToken);

        // Generate appsettings.json
        await GenerateFileAsync("appsettings", Path.Combine(projectDir, "appsettings.json"),
            new { BucketName = options.CouchbaseBucket }, cancellationToken);

        // Generate controller files
        foreach (var entity in options.Entities)
        {
            var controllerModel = new
            {
                Namespace = options.ApiProjectName,
                CoreNamespace = options.CoreProjectName,
                InfrastructureNamespace = options.InfrastructureProjectName,
                Entity = entity
            };

            await GenerateFileAsync("Controller", Path.Combine(controllersDir, $"{entity.NamePlural}Controller.cs"),
                controllerModel, cancellationToken);
        }
    }

    private async Task GenerateFileAsync(string templateName, string outputPath, object model, CancellationToken cancellationToken)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            _logger.LogError("Template not found: {TemplateName}", templateName);
            return;
        }

        var scriptObject = new ScriptObject();
        scriptObject.Import(model, renamer: member => ConvertToSnakeCase(member.Name));

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);
        context.MemberRenamer = member => ConvertToSnakeCase(member.Name);

        var content = await template.RenderAsync(context);
        await File.WriteAllTextAsync(outputPath, content, cancellationToken);

        _logger.LogDebug("Generated: {OutputPath}", outputPath);
    }

    private async Task<string> RenderTemplateAsync(string templateName, object model, CancellationToken cancellationToken)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            _logger.LogError("Template not found: {TemplateName}", templateName);
            return string.Empty;
        }

        var scriptObject = new ScriptObject();
        scriptObject.Import(model, renamer: member => ConvertToSnakeCase(member.Name));

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);
        context.MemberRenamer = member => ConvertToSnakeCase(member.Name);

        return await template.RenderAsync(context);
    }

    /// <inheritdoc />
    public async Task<string> GenerateEntityAsync(SolutionOptions options, EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var entityModel = new
        {
            Namespace = options.CoreProjectName,
            Entity = entity
        };

        return await RenderTemplateAsync("Entity", entityModel, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GenerateRepositoryAsync(SolutionOptions options, EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var repositoryModel = new
        {
            Namespace = options.InfrastructureProjectName,
            CoreNamespace = options.CoreProjectName,
            Entity = entity
        };

        return await RenderTemplateAsync("Repository", repositoryModel, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GenerateControllerAsync(SolutionOptions options, EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var controllerModel = new
        {
            Namespace = options.ApiProjectName,
            CoreNamespace = options.CoreProjectName,
            InfrastructureNamespace = options.InfrastructureProjectName,
            Entity = entity
        };

        return await RenderTemplateAsync("Controller", controllerModel, cancellationToken);
    }

    private static string ConvertToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var result = new System.Text.StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    result.Append('_');
                result.Append(char.ToLowerInvariant(c));
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }
}
