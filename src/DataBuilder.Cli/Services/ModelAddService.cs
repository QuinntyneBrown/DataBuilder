using DataBuilder.Cli.Generators.Angular;
using DataBuilder.Cli.Generators.Api;
using DataBuilder.Cli.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DataBuilder.Cli.Services;

public interface IModelAddService
{
    Task AddModelAsync(SolutionOptions solutionOptions, EntityDefinition entity, CancellationToken cancellationToken);
}

public class ModelAddService : IModelAddService
{
    private readonly IApiGenerator _apiGenerator;
    private readonly IAngularGenerator _angularGenerator;
    private readonly ILogger<ModelAddService> _logger;

    public ModelAddService(
        IApiGenerator apiGenerator,
        IAngularGenerator angularGenerator,
        ILogger<ModelAddService> logger)
    {
        _apiGenerator = apiGenerator;
        _angularGenerator = angularGenerator;
        _logger = logger;
    }

    public async Task AddModelAsync(SolutionOptions solutionOptions, EntityDefinition entity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating API layer files for {Entity}", entity.Name);
        
        // Generate API layer files (Entity, Repository, Controller)
        await GenerateApiFilesAsync(solutionOptions, entity, cancellationToken);

        _logger.LogInformation("Generating Angular layer files for {Entity}", entity.Name);
        
        // Generate Angular layer files (model, service, components)
        await GenerateAngularFilesAsync(solutionOptions, entity, cancellationToken);

        _logger.LogInformation("Updating aggregate files for {Entity}", entity.Name);
        
        // Update aggregate files
        await UpdateServiceCollectionExtensionsAsync(solutionOptions, entity, cancellationToken);
        await UpdateAppRoutesAsync(solutionOptions, entity, cancellationToken);
        await UpdateAppComponentAsync(solutionOptions, entity, cancellationToken);

        _logger.LogInformation("Model {Entity} added successfully", entity.Name);
    }

    private async Task GenerateApiFilesAsync(SolutionOptions solutionOptions, EntityDefinition entity, CancellationToken cancellationToken)
    {
        // Generate Entity model
        var entityPath = Path.Combine(solutionOptions.CoreProjectDirectory, "Models", $"{entity.Name}.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(entityPath)!);
        var entityContent = await _apiGenerator.GenerateEntityAsync(solutionOptions, entity, cancellationToken);
        await File.WriteAllTextAsync(entityPath, entityContent, cancellationToken);
        _logger.LogInformation("Generated entity: {Path}", entityPath);

        // Generate Request DTOs (CreateXRequest, UpdateXRequest)
        var requestsPath = Path.Combine(solutionOptions.CoreProjectDirectory, "Models", $"{entity.Name}Requests.cs");
        var requestsContent = await _apiGenerator.GenerateRequestsAsync(solutionOptions, entity, cancellationToken);
        await File.WriteAllTextAsync(requestsPath, requestsContent, cancellationToken);
        _logger.LogInformation("Generated requests: {Path}", requestsPath);

        // Generate Repository
        var repositoryPath = Path.Combine(solutionOptions.InfrastructureProjectDirectory, "Data", $"{entity.Name}Repository.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(repositoryPath)!);
        var repositoryContent = await _apiGenerator.GenerateRepositoryAsync(solutionOptions, entity, cancellationToken);
        await File.WriteAllTextAsync(repositoryPath, repositoryContent, cancellationToken);
        _logger.LogInformation("Generated repository: {Path}", repositoryPath);

        // Generate Controller
        var controllerPath = Path.Combine(solutionOptions.ApiProjectDirectory, "Controllers", $"{entity.NamePlural}Controller.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(controllerPath)!);
        var controllerContent = await _apiGenerator.GenerateControllerAsync(solutionOptions, entity, cancellationToken);
        await File.WriteAllTextAsync(controllerPath, controllerContent, cancellationToken);
        _logger.LogInformation("Generated controller: {Path}", controllerPath);
    }

    private async Task GenerateAngularFilesAsync(SolutionOptions solutionOptions, EntityDefinition entity, CancellationToken cancellationToken)
    {
        // Generate model
        var modelPath = Path.Combine(solutionOptions.UiProjectDirectory, "src", "app", "models", $"{entity.NameKebabCase}.model.ts");
        Directory.CreateDirectory(Path.GetDirectoryName(modelPath)!);
        var modelContent = await _angularGenerator.GenerateModelAsync(entity, cancellationToken);
        await File.WriteAllTextAsync(modelPath, modelContent, cancellationToken);
        _logger.LogInformation("Generated Angular model: {Path}", modelPath);

        // Generate service
        var servicePath = Path.Combine(solutionOptions.UiProjectDirectory, "src", "app", "services", $"{entity.NameKebabCase}.service.ts");
        Directory.CreateDirectory(Path.GetDirectoryName(servicePath)!);
        var serviceContent = await _angularGenerator.GenerateServiceAsync(entity, cancellationToken);
        await File.WriteAllTextAsync(servicePath, serviceContent, cancellationToken);
        _logger.LogInformation("Generated Angular service: {Path}", servicePath);

        // Generate list component (TypeScript, HTML, SCSS)
        var listComponentDir = Path.Combine(solutionOptions.UiProjectDirectory, "src", "app", "features", entity.NameKebabCase, $"{entity.NameKebabCase}-list");
        Directory.CreateDirectory(listComponentDir);

        var listComponentContent = await _angularGenerator.GenerateListComponentAsync(entity, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(listComponentDir, $"{entity.NameKebabCase}-list.component.ts"), listComponentContent.Ts, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(listComponentDir, $"{entity.NameKebabCase}-list.component.html"), listComponentContent.Html, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(listComponentDir, $"{entity.NameKebabCase}-list.component.scss"), listComponentContent.Css, cancellationToken);
        _logger.LogInformation("Generated list component: {Path}", listComponentDir);

        // Generate detail component (TypeScript, HTML, SCSS)
        var detailComponentDir = Path.Combine(solutionOptions.UiProjectDirectory, "src", "app", "features", entity.NameKebabCase, $"{entity.NameKebabCase}-detail");
        Directory.CreateDirectory(detailComponentDir);

        var detailComponentContent = await _angularGenerator.GenerateDetailComponentAsync(entity, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(detailComponentDir, $"{entity.NameKebabCase}-detail.component.ts"), detailComponentContent.Ts, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(detailComponentDir, $"{entity.NameKebabCase}-detail.component.html"), detailComponentContent.Html, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(detailComponentDir, $"{entity.NameKebabCase}-detail.component.scss"), detailComponentContent.Css, cancellationToken);
        _logger.LogInformation("Generated detail component: {Path}", detailComponentDir);
    }

    private async Task UpdateServiceCollectionExtensionsAsync(SolutionOptions solutionOptions, EntityDefinition entity, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(solutionOptions.InfrastructureProjectDirectory, "ServiceCollectionExtensions.cs");
        
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("ServiceCollectionExtensions.cs not found at {Path}", filePath);
            return;
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // Find the last repository registration and add the new one after it
        var registrationLine = $"        services.AddScoped<I{entity.Name}Repository, {entity.Name}Repository>();";
        
        // Look for the pattern of repository registrations
        var pattern = @"(services\.AddScoped<I\w+Repository, \w+Repository>\(\);)";
        var matches = Regex.Matches(content, pattern);
        
        if (matches.Count > 0)
        {
            // Add after the last repository registration
            var lastMatch = matches[matches.Count - 1];
            var insertPosition = lastMatch.Index + lastMatch.Length;
            content = content.Insert(insertPosition, Environment.NewLine + registrationLine);
        }
        else
        {
            // If no repository registrations found, look for AddInfrastructure method and add there
            var methodPattern = @"(public static IServiceCollection AddInfrastructure\([^)]+\)\s*\{)";
            var methodMatch = Regex.Match(content, methodPattern);
            
            if (methodMatch.Success)
            {
                var insertPosition = methodMatch.Index + methodMatch.Length;
                content = content.Insert(insertPosition, Environment.NewLine + registrationLine + Environment.NewLine);
            }
            else
            {
                _logger.LogWarning("Could not find appropriate location to add repository registration");
                return;
            }
        }

        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        _logger.LogInformation("Updated ServiceCollectionExtensions.cs");
    }

    private async Task UpdateAppRoutesAsync(SolutionOptions solutionOptions, EntityDefinition entity, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(solutionOptions.UiProjectDirectory, "src", "app", "app.routes.ts");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("app.routes.ts not found at {Path}", filePath);
            return;
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);

        // Add route entry with lazy loading pattern (indentation for inside children array)
        var routeEntry = $@"      {{
        path: '{entity.NamePluralKebabCase}',
        loadComponent: () => import('./features/{entity.NameKebabCase}/{entity.NameKebabCase}-list/{entity.NameKebabCase}-list.component')
          .then(m => m.{entity.Name}ListComponent)
      }},
      {{
        path: '{entity.NamePluralKebabCase}/new',
        loadComponent: () => import('./features/{entity.NameKebabCase}/{entity.NameKebabCase}-detail/{entity.NameKebabCase}-detail.component')
          .then(m => m.{entity.Name}DetailComponent)
      }},
      {{
        path: '{entity.NamePluralKebabCase}/:id',
        loadComponent: () => import('./features/{entity.NameKebabCase}/{entity.NameKebabCase}-detail/{entity.NameKebabCase}-detail.component')
          .then(m => m.{entity.Name}DetailComponent)
      }},";

        // Find where to insert the route (before the default redirect route in children array)
        var redirectPattern = @"(\s+)\{\s*path:\s*'',\s*redirectTo:";
        var redirectMatch = Regex.Match(content, redirectPattern);

        if (redirectMatch.Success)
        {
            var insertPosition = redirectMatch.Index;
            content = content.Insert(insertPosition, routeEntry + Environment.NewLine);
        }

        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        _logger.LogInformation("Updated app.routes.ts");
    }

    private async Task UpdateAppComponentAsync(SolutionOptions solutionOptions, EntityDefinition entity, CancellationToken cancellationToken)
    {
        // Update the main-layout component which contains the navigation
        var filePath = Path.Combine(solutionOptions.UiProjectDirectory, "src", "app", "layouts", "main-layout", "main-layout.component.ts");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("main-layout.component.ts not found at {Path}", filePath);
            return;
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);

        // Add navigation item to the mat-nav-list
        var navItem = $@"              <a mat-list-item routerLink=""/{entity.NamePluralKebabCase}"" routerLinkActive=""active-link"">
                <mat-icon matListItemIcon>{entity.Icon}</mat-icon>
                <span matListItemTitle>{entity.DisplayNamePlural}</span>
              </a>";

        // Find the </mat-nav-list> closing tag
        var navListPattern = @"(\s*</mat-nav-list>)";
        var navListMatch = Regex.Match(content, navListPattern);

        if (navListMatch.Success)
        {
            var insertPosition = navListMatch.Index;
            content = content.Insert(insertPosition, navItem + Environment.NewLine);
        }

        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        _logger.LogInformation("Updated main-layout.component.ts");
    }
}
