using System.Reflection;
using DataBuilder.Cli.Models;
using DataBuilder.Cli.Utilities;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;

namespace DataBuilder.Cli.Generators.Angular;

/// <summary>
/// Generates the Angular frontend project.
/// </summary>
public class AngularGenerator : IAngularGenerator
{
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<AngularGenerator> _logger;
    private readonly Dictionary<string, Template> _templates = new();

    public AngularGenerator(IProcessRunner processRunner, ILogger<AngularGenerator> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
        LoadTemplates();
    }

    private void LoadTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templateNames = new[]
        {
            "model-ts", "service-ts", "list.component", "detail.component",
            "styles.scss", "index.html", "app.routes", "app.component", "app.config",
            "main-layout.component"
        };

        foreach (var name in templateNames)
        {
            var resourceName = $"DataBuilder.Cli.Templates.Angular.{name}.sbn";
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
        _logger.LogInformation("Generating Angular project: {ProjectName}", options.UiProjectName);

        // Create Angular project using ng CLI
        await CreateAngularProjectAsync(options, cancellationToken);

        // Add Angular Material
        await AddAngularMaterialAsync(options, cancellationToken);

        // Generate custom code
        await GenerateCustomCodeAsync(options, cancellationToken);

        _logger.LogInformation("Angular project generation complete");
    }

    private async Task CreateAngularProjectAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Angular project with ng CLI...");

        var result = await _processRunner.RunWithOutputAsync(
            "ng",
            $"new {options.UiProjectName} --style=scss --routing=true --skip-git --skip-install --standalone",
            options.SrcDirectory,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("ng new failed (may already exist): {Error}", result.StandardError);
            // Create directory structure manually if ng CLI fails
            await CreateManualAngularStructureAsync(options, cancellationToken);
        }
        else
        {
            // Ensure @angular/animations is in package.json (ng new may not include it)
            await EnsureAnimationsPackageAsync(options, cancellationToken);
            // Ensure zone.js is in package.json
            await EnsureZoneJsPackageAsync(options, cancellationToken);
            // Ensure zone.js is imported in main.ts
            await EnsureZoneJsImportAsync(options, cancellationToken);
            // Ensure vanilla-jsoneditor is in package.json for JSON editing
            await EnsureJsonEditorPackageAsync(options, cancellationToken);
            // Ensure vanilla-jsoneditor dark theme CSS is in angular.json styles
            await EnsureJsonEditorStylesAsync(options, cancellationToken);
        }
    }

    private async Task EnsureAnimationsPackageAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        var packageJsonPath = Path.Combine(options.UiProjectDirectory, "package.json");
        if (!File.Exists(packageJsonPath))
            return;

        var content = await File.ReadAllTextAsync(packageJsonPath, cancellationToken);

        // Check if @angular/animations is already present
        if (content.Contains("@angular/animations"))
            return;

        _logger.LogInformation("Adding @angular/animations to package.json...");

        // Find the dependencies section and add @angular/animations
        // Look for "@angular/common" and add animations before it
        var commonPattern = "\"@angular/common\"";
        var commonIndex = content.IndexOf(commonPattern);
        if (commonIndex > 0)
        {
            // Extract the version pattern from @angular/common
            var versionStart = content.IndexOf(':', commonIndex) + 1;
            var versionEnd = content.IndexOf(',', versionStart);
            if (versionEnd < 0) versionEnd = content.IndexOf('}', versionStart);
            var version = content.Substring(versionStart, versionEnd - versionStart).Trim().Trim('"');

            // Insert @angular/animations before @angular/common
            var insertText = $"\"@angular/animations\": \"{version}\",\n    ";
            content = content.Insert(commonIndex, insertText);

            await File.WriteAllTextAsync(packageJsonPath, content, cancellationToken);
            _logger.LogDebug("Added @angular/animations to package.json");
        }
    }

    private async Task EnsureZoneJsPackageAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        var packageJsonPath = Path.Combine(options.UiProjectDirectory, "package.json");
        if (!File.Exists(packageJsonPath))
            return;

        var content = await File.ReadAllTextAsync(packageJsonPath, cancellationToken);

        // Check if zone.js is already present
        if (content.Contains("\"zone.js\""))
            return;

        _logger.LogInformation("Adding zone.js to package.json...");

        // Find the dependencies section and add zone.js
        // Look for "tslib" and add zone.js after it
        var tslibPattern = "\"tslib\"";
        var tslibIndex = content.IndexOf(tslibPattern);
        if (tslibIndex > 0)
        {
            // Find the end of the tslib line
            var lineEnd = content.IndexOf('\n', tslibIndex);
            if (lineEnd > 0)
            {
                // Insert zone.js after tslib
                var insertText = ",\n    \"zone.js\": \"~0.15.0\"";
                content = content.Insert(lineEnd, insertText);

                await File.WriteAllTextAsync(packageJsonPath, content, cancellationToken);
                _logger.LogDebug("Added zone.js to package.json");
            }
        }
    }

    private async Task EnsureZoneJsImportAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        var mainTsPath = Path.Combine(options.UiProjectDirectory, "src", "main.ts");
        if (!File.Exists(mainTsPath))
            return;

        var content = await File.ReadAllTextAsync(mainTsPath, cancellationToken);

        // Check if zone.js is already imported
        if (content.Contains("import 'zone.js'") || content.Contains("import \"zone.js\""))
            return;

        _logger.LogInformation("Adding zone.js import to main.ts...");

        // Add zone.js import at the beginning of the file
        content = "import 'zone.js';\n" + content;

        await File.WriteAllTextAsync(mainTsPath, content, cancellationToken);
        _logger.LogDebug("Added zone.js import to main.ts");
    }

    private async Task EnsureJsonEditorPackageAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        var packageJsonPath = Path.Combine(options.UiProjectDirectory, "package.json");
        if (!File.Exists(packageJsonPath))
            return;

        var content = await File.ReadAllTextAsync(packageJsonPath, cancellationToken);

        // Check if vanilla-jsoneditor is already present
        if (content.Contains("vanilla-jsoneditor"))
            return;

        _logger.LogInformation("Adding vanilla-jsoneditor to package.json...");

        // Find the dependencies section and add vanilla-jsoneditor before zone.js (which should be last)
        // If zone.js exists, insert before it; otherwise insert before devDependencies
        var zoneJsPattern = "\"zone.js\"";
        var zoneJsIndex = content.IndexOf(zoneJsPattern);
        if (zoneJsIndex > 0)
        {
            // Find the start of the zone.js line (go back to find the whitespace)
            var lineStart = content.LastIndexOf('\n', zoneJsIndex);
            if (lineStart > 0)
            {
                // Insert vanilla-jsoneditor before zone.js
                var insertText = "\"vanilla-jsoneditor\": \"^1.0.0\",\n    ";
                content = content.Insert(lineStart + 1 + 4, insertText); // +1 for newline, +4 for indentation

                await File.WriteAllTextAsync(packageJsonPath, content, cancellationToken);
                _logger.LogDebug("Added vanilla-jsoneditor to package.json");
            }
        }
    }

    private async Task EnsureJsonEditorStylesAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        var angularJsonPath = Path.Combine(options.UiProjectDirectory, "angular.json");
        if (!File.Exists(angularJsonPath))
            return;

        var content = await File.ReadAllTextAsync(angularJsonPath, cancellationToken);

        // Check if vanilla-jsoneditor theme is already present
        if (content.Contains("vanilla-jsoneditor"))
            return;

        _logger.LogInformation("Adding vanilla-jsoneditor dark theme to angular.json...");

        // Find the styles array and add the dark theme CSS
        var stylesPattern = "\"src/styles.scss\"";
        var stylesIndex = content.IndexOf(stylesPattern);
        if (stylesIndex > 0)
        {
            // Insert the dark theme CSS after styles.scss
            var insertPosition = stylesIndex + stylesPattern.Length;
            var insertText = ",\n              \"node_modules/vanilla-jsoneditor/themes/jse-theme-dark.css\"";
            content = content.Insert(insertPosition, insertText);

            await File.WriteAllTextAsync(angularJsonPath, content, cancellationToken);
            _logger.LogDebug("Added vanilla-jsoneditor dark theme to angular.json");
        }
    }

    private async Task CreateManualAngularStructureAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Angular project structure manually...");

        var projectDir = options.UiProjectDirectory;
        var srcDir = Path.Combine(projectDir, "src");
        var appDir = Path.Combine(srcDir, "app");

        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(Path.Combine(appDir, "models"));
        Directory.CreateDirectory(Path.Combine(appDir, "services"));
        Directory.CreateDirectory(Path.Combine(appDir, "features"));

        // Create package.json
        var packageJson = $@"{{
  ""name"": ""{options.NameKebabCase}"",
  ""version"": ""0.0.0"",
  ""scripts"": {{
    ""ng"": ""ng"",
    ""start"": ""ng serve"",
    ""build"": ""ng build"",
    ""test"": ""ng test""
  }},
  ""private"": true,
  ""dependencies"": {{
    ""@angular/animations"": ""^19.0.0"",
    ""@angular/cdk"": ""^19.0.0"",
    ""@angular/common"": ""^19.0.0"",
    ""@angular/compiler"": ""^19.0.0"",
    ""@angular/core"": ""^19.0.0"",
    ""@angular/forms"": ""^19.0.0"",
    ""@angular/material"": ""^19.0.0"",
    ""@angular/platform-browser"": ""^19.0.0"",
    ""@angular/platform-browser-dynamic"": ""^19.0.0"",
    ""@angular/router"": ""^19.0.0"",
    ""rxjs"": ""~7.8.0"",
    ""tslib"": ""^2.3.0"",
    ""vanilla-jsoneditor"": ""^1.0.0"",
    ""zone.js"": ""~0.15.0""
  }},
  ""devDependencies"": {{
    ""@angular-devkit/build-angular"": ""^19.0.0"",
    ""@angular/cli"": ""^19.0.0"",
    ""@angular/compiler-cli"": ""^19.0.0"",
    ""typescript"": ""~5.6.0""
  }}
}}";
        await File.WriteAllTextAsync(Path.Combine(projectDir, "package.json"), packageJson, cancellationToken);

        // Create angular.json
        var angularJson = $@"{{
  ""$schema"": ""./node_modules/@angular/cli/lib/config/schema.json"",
  ""version"": 1,
  ""newProjectRoot"": ""projects"",
  ""projects"": {{
    ""{options.NameKebabCase}"": {{
      ""projectType"": ""application"",
      ""root"": """",
      ""sourceRoot"": ""src"",
      ""prefix"": ""app"",
      ""architect"": {{
        ""build"": {{
          ""builder"": ""@angular-devkit/build-angular:application"",
          ""options"": {{
            ""outputPath"": ""dist/{options.NameKebabCase}"",
            ""index"": ""src/index.html"",
            ""browser"": ""src/main.ts"",
            ""polyfills"": [""zone.js""],
            ""tsConfig"": ""tsconfig.app.json"",
            ""inlineStyleLanguage"": ""scss"",
            ""styles"": [""src/styles.scss"", ""node_modules/vanilla-jsoneditor/themes/jse-theme-dark.css""],
            ""scripts"": []
          }}
        }},
        ""serve"": {{
          ""builder"": ""@angular-devkit/build-angular:dev-server"",
          ""configurations"": {{
            ""development"": {{
              ""buildTarget"": ""{options.NameKebabCase}:build:development""
            }}
          }},
          ""defaultConfiguration"": ""development""
        }}
      }}
    }}
  }}
}}";
        await File.WriteAllTextAsync(Path.Combine(projectDir, "angular.json"), angularJson, cancellationToken);

        // Create tsconfig.json
        var tsConfig = @"{
  ""compileOnSave"": false,
  ""compilerOptions"": {
    ""outDir"": ""./dist/out-tsc"",
    ""strict"": true,
    ""noImplicitOverride"": true,
    ""noPropertyAccessFromIndexSignature"": true,
    ""noImplicitReturns"": true,
    ""noFallthroughCasesInSwitch"": true,
    ""skipLibCheck"": true,
    ""esModuleInterop"": true,
    ""sourceMap"": true,
    ""declaration"": false,
    ""experimentalDecorators"": true,
    ""moduleResolution"": ""bundler"",
    ""importHelpers"": true,
    ""target"": ""ES2022"",
    ""module"": ""ES2022"",
    ""lib"": [""ES2022"", ""dom""]
  },
  ""angularCompilerOptions"": {
    ""enableI18nLegacyMessageIdFormat"": false,
    ""strictInjectionParameters"": true,
    ""strictInputAccessModifiers"": true,
    ""strictTemplates"": true
  }
}";
        await File.WriteAllTextAsync(Path.Combine(projectDir, "tsconfig.json"), tsConfig, cancellationToken);

        // Create tsconfig.app.json
        var tsConfigApp = @"{
  ""extends"": ""./tsconfig.json"",
  ""compilerOptions"": {
    ""outDir"": ""./out-tsc/app"",
    ""types"": []
  },
  ""files"": [""src/main.ts""],
  ""include"": [""src/**/*.d.ts""]
}";
        await File.WriteAllTextAsync(Path.Combine(projectDir, "tsconfig.app.json"), tsConfigApp, cancellationToken);

        // Create main.ts
        var mainTs = @"import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
";
        await File.WriteAllTextAsync(Path.Combine(srcDir, "main.ts"), mainTs, cancellationToken);
    }

    private async Task AddAngularMaterialAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding Angular Material...");

        var result = await _processRunner.RunAsync(
            "ng",
            "add @angular/material --skip-confirmation --theme=custom --typography=true --animations=enabled",
            options.UiProjectDirectory,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("ng add @angular/material failed (may need npm install first): {Error}", result.StandardError);
        }
    }

    private async Task GenerateCustomCodeAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        var srcDir = Path.Combine(options.UiProjectDirectory, "src");
        var appDir = Path.Combine(srcDir, "app");

        // Ensure directories exist
        Directory.CreateDirectory(Path.Combine(appDir, "models"));
        Directory.CreateDirectory(Path.Combine(appDir, "services"));
        Directory.CreateDirectory(Path.Combine(appDir, "layouts", "main-layout"));

        // Generate styles.scss
        await GenerateFileAsync("styles.scss", Path.Combine(srcDir, "styles.scss"),
            new { }, cancellationToken);

        // Generate index.html
        await GenerateFileAsync("index.html", Path.Combine(srcDir, "index.html"),
            new { SolutionName = options.NamePascalCase }, cancellationToken);

        // Clean up default Angular CLI app template - replace with router-outlet only
        var appHtmlPath = Path.Combine(appDir, "app.html");
        if (File.Exists(appHtmlPath))
        {
            await File.WriteAllTextAsync(appHtmlPath, "<router-outlet />\n", cancellationToken);
        }

        // Generate app.routes.ts
        await GenerateFileAsync("app.routes", Path.Combine(appDir, "app.routes.ts"),
            new { Entities = options.Entities }, cancellationToken);

        // Generate app.component.ts
        await GenerateFileAsync("app.component", Path.Combine(appDir, "app.component.ts"),
            new { SolutionName = options.NamePascalCase, Entities = options.Entities }, cancellationToken);

        // Generate app.config.ts
        await GenerateFileAsync("app.config", Path.Combine(appDir, "app.config.ts"),
            new { }, cancellationToken);

        // Generate main-layout component
        await GenerateFileAsync("main-layout.component",
            Path.Combine(appDir, "layouts", "main-layout", "main-layout.component.ts"),
            new { SolutionName = options.NamePascalCase, Entities = options.Entities },
            cancellationToken);

        // Generate entity-specific files
        foreach (var entity in options.Entities)
        {
            var entityModel = new
            {
                Entity = entity,
                ApiPort = options.ApiPort
            };

            // Model
            await GenerateFileAsync("model-ts", Path.Combine(appDir, "models", $"{entity.NameKebabCase}.model.ts"),
                entityModel, cancellationToken);

            // Service
            await GenerateFileAsync("service-ts", Path.Combine(appDir, "services", $"{entity.NameKebabCase}.service.ts"),
                entityModel, cancellationToken);

            // Feature components
            var featureDir = Path.Combine(appDir, "features", entity.NameKebabCase);
            var listDir = Path.Combine(featureDir, $"{entity.NameKebabCase}-list");
            var detailDir = Path.Combine(featureDir, $"{entity.NameKebabCase}-detail");

            Directory.CreateDirectory(listDir);
            Directory.CreateDirectory(detailDir);

            // List component
            await GenerateFileAsync("list.component", Path.Combine(listDir, $"{entity.NameKebabCase}-list.component.ts"),
                entityModel, cancellationToken);

            // Detail component
            await GenerateFileAsync("detail.component", Path.Combine(detailDir, $"{entity.NameKebabCase}-detail.component.ts"),
                entityModel, cancellationToken);
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
    public async Task<string> GenerateModelAsync(EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var model = new { Entity = entity };
        return await RenderTemplateAsync("model-ts", model, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GenerateServiceAsync(EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var model = new { Entity = entity, ApiPort = 5000 }; // Default port
        return await RenderTemplateAsync("service-ts", model, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ComponentContent> GenerateListComponentAsync(EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var model = new { Entity = entity };
        var ts = await RenderTemplateAsync("list.component", model, cancellationToken);
        // Components use inline templates/styles, so HTML and CSS are empty strings
        return new ComponentContent(ts, string.Empty, string.Empty);
    }

    /// <inheritdoc />
    public async Task<ComponentContent> GenerateDetailComponentAsync(EntityDefinition entity, CancellationToken cancellationToken = default)
    {
        var model = new { Entity = entity };
        var ts = await RenderTemplateAsync("detail.component", model, cancellationToken);
        // Components use inline templates/styles, so HTML and CSS are empty strings
        return new ComponentContent(ts, string.Empty, string.Empty);
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
