# Maintainability Improvements Analysis

**Project:** DataBuilder.Cli  
**Analysis Date:** February 2, 2026  
**Analyst:** GitHub Copilot  
**Scope:** Code quality, design patterns, and maintainability improvements

---

## Executive Summary

This document provides a supplementary analysis to the existing `CODE-QUALITY-ANALYSIS.md`, focusing on actionable improvements with concrete implementation examples. The codebase demonstrates good practices in several areas (dependency injection, interface segregation, async patterns), but there are opportunities to enhance maintainability and reduce technical debt.

---

## Table of Contents

1. [Strengths of the Current Codebase](#1-strengths-of-the-current-codebase)
2. [Architecture Improvements](#2-architecture-improvements)
3. [Design Pattern Recommendations](#3-design-pattern-recommendations)
4. [Error Handling Strategy](#4-error-handling-strategy)
5. [Async/Await Best Practices](#5-asyncawait-best-practices)
6. [Configuration Management](#6-configuration-management)
7. [Testing Strategy](#7-testing-strategy)
8. [Performance Considerations](#8-performance-considerations)
9. [Code Organization](#9-code-organization)
10. [Implementation Roadmap](#10-implementation-roadmap)

---

## 1. Strengths of the Current Codebase

Before diving into improvements, it's worth recognizing what's done well:

| Aspect | Implementation | Grade |
|--------|---------------|-------|
| **Dependency Injection** | Services registered with proper lifetimes | ✅ Good |
| **Interface Abstraction** | All services have interfaces | ✅ Good |
| **Async Patterns** | Consistent use of async/await with CancellationToken | ✅ Good |
| **Nullable Reference Types** | Enabled with `<Nullable>enable</Nullable>` | ✅ Good |
| **Modern C# Features** | Uses records, pattern matching, target-typed new | ✅ Good |
| **Logging** | Structured logging with ILogger throughout | ✅ Good |
| **Template Engine** | Scriban for code generation (maintainable) | ✅ Good |
| **CLI Framework** | System.CommandLine for robust parsing | ✅ Good |

---

## 2. Architecture Improvements

### 2.1 Command Handler Pattern Refinement

**Current State:**  
Command handlers are defined in the same file as command definitions, creating large files and mixed concerns.

**Recommendation:** Separate commands from handlers for cleaner architecture:

```
src/DataBuilder.Cli/
├── Commands/
│   ├── Definitions/
│   │   ├── ModelAddCommand.cs
│   │   └── SolutionCreateCommand.cs
│   ├── Handlers/
│   │   ├── ModelAddCommandHandler.cs
│   │   └── SolutionCreateCommandHandler.cs
│   └── Options/
│       ├── CouchbaseOptions.cs
│       └── GenerationOptions.cs
```

### 2.2 Introduce Result Pattern

**Current State:**  
Methods return `Task<int>` for exit codes, mixing success/failure with return values.

**Recommendation:** Implement the Result pattern for cleaner error handling:

```csharp
// New file: src/DataBuilder.Cli/Common/Result.cs
namespace DataBuilder.Cli.Common;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public Exception? Exception { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
    public static Result<T> Failure(Exception ex) => new() { IsSuccess = false, Error = ex.Message, Exception = ex };

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

public record Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, Error = error };
}
```

**Usage:**

```csharp
public async Task<Result<EntityDefinition>> ParseEntityAsync(string json)
{
    try
    {
        var entity = _schemaParser.Parse(json);
        return Result<EntityDefinition>.Success(entity);
    }
    catch (JsonException ex)
    {
        return Result<EntityDefinition>.Failure($"Invalid JSON: {ex.Message}");
    }
}
```

### 2.3 Pipeline Pattern for Generation

**Current State:**  
`ModelAddService` and `SolutionGenerator` have procedural generation logic.

**Recommendation:** Implement a pipeline pattern for extensibility:

```csharp
// New file: src/DataBuilder.Cli/Pipeline/IGenerationStep.cs
namespace DataBuilder.Cli.Pipeline;

public interface IGenerationStep
{
    string Name { get; }
    int Order { get; }
    Task<Result> ExecuteAsync(GenerationContext context, CancellationToken ct);
}

public class GenerationContext
{
    public SolutionOptions Options { get; init; } = null!;
    public EntityDefinition? Entity { get; set; }
    public Dictionary<string, object> State { get; } = new();
}

// Example step
public class GenerateEntityStep : IGenerationStep
{
    private readonly IApiGenerator _apiGenerator;
    
    public string Name => "Generate Entity Model";
    public int Order => 10;

    public async Task<Result> ExecuteAsync(GenerationContext context, CancellationToken ct)
    {
        var content = await _apiGenerator.GenerateEntityAsync(context.Options, context.Entity!, ct);
        var path = Path.Combine(context.Options.CoreProjectDirectory, "Models", $"{context.Entity!.Name}.cs");
        await File.WriteAllTextAsync(path, content, ct);
        return Result.Success();
    }
}

// Pipeline executor
public class GenerationPipeline
{
    private readonly IEnumerable<IGenerationStep> _steps;
    private readonly ILogger<GenerationPipeline> _logger;

    public GenerationPipeline(IEnumerable<IGenerationStep> steps, ILogger<GenerationPipeline> logger)
    {
        _steps = steps.OrderBy(s => s.Order);
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(GenerationContext context, CancellationToken ct)
    {
        foreach (var step in _steps)
        {
            _logger.LogInformation("Executing step: {StepName}", step.Name);
            var result = await step.ExecuteAsync(context, ct);
            if (!result.IsSuccess)
            {
                _logger.LogError("Step failed: {StepName} - {Error}", step.Name, result.Error);
                return result;
            }
        }
        return Result.Success();
    }
}
```

---

## 3. Design Pattern Recommendations

### 3.1 Factory Pattern for Template Loading

**Current State:**  
Templates are loaded inline in generator constructors.

**Recommendation:**

```csharp
// New file: src/DataBuilder.Cli/Templates/ITemplateFactory.cs
namespace DataBuilder.Cli.Templates;

public interface ITemplateFactory
{
    Template GetTemplate(string name);
    bool HasTemplate(string name);
    IReadOnlyList<string> AvailableTemplates { get; }
}

public class EmbeddedResourceTemplateFactory : ITemplateFactory
{
    private readonly Lazy<Dictionary<string, Template>> _templates;
    private readonly ILogger<EmbeddedResourceTemplateFactory> _logger;

    public EmbeddedResourceTemplateFactory(ILogger<EmbeddedResourceTemplateFactory> logger)
    {
        _logger = logger;
        _templates = new Lazy<Dictionary<string, Template>>(LoadAllTemplates);
    }

    public Template GetTemplate(string name)
    {
        if (!_templates.Value.TryGetValue(name, out var template))
        {
            throw new TemplateNotFoundException(name);
        }
        return template;
    }

    public bool HasTemplate(string name) => _templates.Value.ContainsKey(name);

    public IReadOnlyList<string> AvailableTemplates => _templates.Value.Keys.ToList();

    private Dictionary<string, Template> LoadAllTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templates = new Dictionary<string, Template>();

        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sbn")))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            
            var templateName = ExtractTemplateName(resourceName);
            var content = reader.ReadToEnd();
            var template = Template.Parse(content);

            if (template.HasErrors)
            {
                _logger.LogError("Template {Name} has errors: {Errors}", 
                    templateName, string.Join(", ", template.Messages));
                continue;
            }

            templates[templateName] = template;
            _logger.LogDebug("Loaded template: {Name}", templateName);
        }

        return templates;
    }

    private static string ExtractTemplateName(string resourceName)
    {
        // DataBuilder.Cli.Templates.Api.Entity.sbn -> Entity
        var parts = resourceName.Split('.');
        return parts[^2]; // Second to last
    }
}
```

### 3.2 Strategy Pattern for Editor Selection

**Current State:**  
Editor selection uses hardcoded switch expressions in `JsonEditorService`.

**Recommendation:**

```csharp
// New file: src/DataBuilder.Cli/Editors/IEditorStrategy.cs
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

// Selector service
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
```

### 3.3 Builder Pattern for SolutionOptions

**Current State:**  
`SolutionOptions` is a mutable class with many properties.

**Recommendation:**

```csharp
// Enhanced SolutionOptions with builder
public class SolutionOptionsBuilder
{
    private string _name = string.Empty;
    private string _directory = string.Empty;
    private readonly List<EntityDefinition> _entities = new();
    private string _bucket = "general";
    private string _scope = "general";

    public SolutionOptionsBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SolutionOptionsBuilder WithDirectory(string directory)
    {
        _directory = directory;
        return this;
    }

    public SolutionOptionsBuilder AddEntity(EntityDefinition entity)
    {
        _entities.Add(entity);
        return this;
    }

    public SolutionOptionsBuilder AddEntities(IEnumerable<EntityDefinition> entities)
    {
        _entities.AddRange(entities);
        return this;
    }

    public SolutionOptionsBuilder WithCouchbase(string bucket, string scope)
    {
        _bucket = bucket;
        _scope = scope;
        return this;
    }

    public SolutionOptions Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Solution name is required");

        return new SolutionOptions
        {
            Name = _name,
            Directory = _directory,
            Entities = _entities,
            DefaultBucket = _bucket,
            DefaultScope = _scope
        };
    }
}

// Usage
var options = new SolutionOptionsBuilder()
    .WithName("MyApp")
    .WithDirectory("C:/Projects")
    .AddEntities(entities)
    .WithCouchbase("data", "main")
    .Build();
```

---

## 4. Error Handling Strategy

### 4.1 Custom Exception Hierarchy

```csharp
// New file: src/DataBuilder.Cli/Exceptions/DataBuilderExceptions.cs
namespace DataBuilder.Cli.Exceptions;

/// <summary>
/// Base exception for all DataBuilder operations.
/// </summary>
public abstract class DataBuilderException : Exception
{
    public string? Context { get; init; }
    
    protected DataBuilderException(string message) : base(message) { }
    protected DataBuilderException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when JSON schema parsing fails.
/// </summary>
public class SchemaParseException : DataBuilderException
{
    public string? JsonContent { get; init; }
    
    public SchemaParseException(string message) : base(message) { }
    public SchemaParseException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a required template is not found.
/// </summary>
public class TemplateNotFoundException : DataBuilderException
{
    public string TemplateName { get; }
    
    public TemplateNotFoundException(string templateName) 
        : base($"Template not found: {templateName}")
    {
        TemplateName = templateName;
    }
}

/// <summary>
/// Thrown when template rendering fails.
/// </summary>
public class TemplateRenderException : DataBuilderException
{
    public string TemplateName { get; }
    
    public TemplateRenderException(string templateName, Exception inner) 
        : base($"Failed to render template: {templateName}", inner)
    {
        TemplateName = templateName;
    }
}

/// <summary>
/// Thrown when solution structure validation fails.
/// </summary>
public class InvalidSolutionStructureException : DataBuilderException
{
    public string SolutionPath { get; }
    
    public InvalidSolutionStructureException(string solutionPath, string message) 
        : base(message)
    {
        SolutionPath = solutionPath;
    }
}

/// <summary>
/// Thrown when external process execution fails.
/// </summary>
public class ProcessExecutionException : DataBuilderException
{
    public string Command { get; }
    public int ExitCode { get; }
    public string? StandardError { get; }
    
    public ProcessExecutionException(string command, int exitCode, string? stderr) 
        : base($"Command '{command}' failed with exit code {exitCode}")
    {
        Command = command;
        ExitCode = exitCode;
        StandardError = stderr;
    }
}
```

### 4.2 Global Exception Handler

```csharp
// New file: src/DataBuilder.Cli/ExceptionHandler.cs
namespace DataBuilder.Cli;

public static class ExceptionHandler
{
    public static int Handle(Exception ex, ILogger logger)
    {
        switch (ex)
        {
            case SchemaParseException spe:
                logger.LogError("Invalid JSON schema: {Message}", spe.Message);
                Console.Error.WriteLine($"Error: Invalid JSON schema - {spe.Message}");
                return 2;

            case TemplateNotFoundException tnfe:
                logger.LogError(tnfe, "Missing template: {Template}", tnfe.TemplateName);
                Console.Error.WriteLine($"Error: Required template missing - {tnfe.TemplateName}");
                return 3;

            case InvalidSolutionStructureException isse:
                logger.LogError("Invalid solution: {Path} - {Message}", isse.SolutionPath, isse.Message);
                Console.Error.WriteLine($"Error: {isse.Message}");
                return 4;

            case ProcessExecutionException pee:
                logger.LogError(pee, "Process failed: {Command}", pee.Command);
                Console.Error.WriteLine($"Error: {pee.Command} failed");
                if (!string.IsNullOrEmpty(pee.StandardError))
                    Console.Error.WriteLine(pee.StandardError);
                return 5;

            case OperationCanceledException:
                logger.LogWarning("Operation cancelled");
                Console.Error.WriteLine("Operation cancelled.");
                return 130; // Standard SIGINT exit code

            default:
                logger.LogError(ex, "Unexpected error");
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                return 1;
        }
    }
}
```

---

## 5. Async/Await Best Practices

### 5.1 ConfigureAwait Considerations

For CLI applications, `ConfigureAwait(false)` can be used for performance (no UI context):

```csharp
// In library code (services, generators)
public async Task<string> GenerateEntityAsync(...)
{
    var content = await RenderTemplateAsync(...).ConfigureAwait(false);
    await File.WriteAllTextAsync(path, content, ct).ConfigureAwait(false);
    return content;
}
```

### 5.2 Timeout Patterns

Add timeouts to external process calls:

```csharp
public async Task<ProcessResult> RunWithTimeoutAsync(
    string command, 
    string arguments, 
    string workingDirectory,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
{
    using var timeoutCts = new CancellationTokenSource(timeout);
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken, timeoutCts.Token);

    try
    {
        return await RunAsync(command, arguments, workingDirectory, linkedCts.Token);
    }
    catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
    {
        throw new TimeoutException($"Command '{command}' timed out after {timeout.TotalSeconds}s");
    }
}
```

### 5.3 Parallel Generation

For independent file generation:

```csharp
public async Task GenerateEntitiesAsync(
    IReadOnlyList<EntityDefinition> entities,
    CancellationToken ct)
{
    // Generate entities in parallel (they're independent)
    await Parallel.ForEachAsync(entities, ct, async (entity, token) =>
    {
        await GenerateEntityAsync(entity, token);
    });
}
```

---

## 6. Configuration Management

### 6.1 Options Pattern

```csharp
// New file: src/DataBuilder.Cli/Configuration/DataBuilderOptions.cs
namespace DataBuilder.Cli.Configuration;

public class DataBuilderOptions
{
    public const string SectionName = "DataBuilder";

    public CouchbaseDefaults Couchbase { get; set; } = new();
    public EditorOptions Editor { get; set; } = new();
    public AngularOptions Angular { get; set; } = new();
}

public class CouchbaseDefaults
{
    public string Bucket { get; set; } = "general";
    public string Scope { get; set; } = "general";
    public bool UseTypeDiscriminator { get; set; } = false;
}

public class EditorOptions
{
    public string? PreferredEditor { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);
}

public class AngularOptions
{
    public int DevServerPort { get; set; } = 4200;
    public bool SkipInstall { get; set; } = true;
    public bool AddMaterial { get; set; } = true;
}
```

### 6.2 Configuration File Support

```csharp
// In Program.cs
static void ConfigureServices(IServiceCollection services)
{
    // Load configuration from file or defaults
    var configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".databuilder",
        "config.json");

    var configuration = new ConfigurationBuilder()
        .AddJsonFile(configPath, optional: true)
        .AddEnvironmentVariables("DATABUILDER_")
        .Build();

    services.Configure<DataBuilderOptions>(
        configuration.GetSection(DataBuilderOptions.SectionName));

    // ... rest of services
}
```

---

## 7. Testing Strategy

### 7.1 Missing Test Coverage

| Component | Current Coverage | Recommended Tests |
|-----------|------------------|-------------------|
| `SchemaParser` | Good | Add edge cases, malformed JSON |
| `ApiGenerator` | None | Template rendering, file output |
| `AngularGenerator` | None | Component generation, file structure |
| `ModelAddService` | None | Integration tests with mock file system |
| `Command Handlers` | Basic | End-to-end CLI tests |
| `NamingConventions` | Unknown | All transformation methods |
| `TypeMapper` | Unknown | All type mappings |

### 7.2 Test Infrastructure Recommendations

```csharp
// New file: tests/DataBuilder.Cli.Tests/TestInfrastructure/InMemoryFileSystem.cs
public class InMemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new();
    private readonly HashSet<string> _directories = new();

    public Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
    {
        if (!_files.TryGetValue(NormalizePath(path), out var content))
            throw new FileNotFoundException(path);
        return Task.FromResult(content);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
    {
        _files[NormalizePath(path)] = content;
        return Task.CompletedTask;
    }

    public bool FileExists(string path) => _files.ContainsKey(NormalizePath(path));

    public void CreateDirectory(string path) => _directories.Add(NormalizePath(path));

    public IReadOnlyDictionary<string, string> Files => _files;

    private static string NormalizePath(string path) => path.Replace('\\', '/').ToLowerInvariant();
}
```

### 7.3 Generator Integration Tests

```csharp
[Fact]
public async Task GenerateAsync_CreatesCompleteProjectStructure()
{
    // Arrange
    var fileSystem = new InMemoryFileSystem();
    var templateFactory = new EmbeddedResourceTemplateFactory(NullLogger<...>.Instance);
    var generator = new ApiGenerator(fileSystem, templateFactory, NullLogger<ApiGenerator>.Instance);

    var options = new SolutionOptionsBuilder()
        .WithName("TestApp")
        .WithDirectory("/test")
        .AddEntity(new EntityDefinition { Name = "Product", Properties = [...] })
        .Build();

    // Act
    await generator.GenerateAsync(options, CancellationToken.None);

    // Assert
    fileSystem.FileExists("/test/TestApp/src/TestApp.Core/Models/Product.cs").Should().BeTrue();
    fileSystem.FileExists("/test/TestApp/src/TestApp.Core/TestApp.Core.csproj").Should().BeTrue();
    fileSystem.FileExists("/test/TestApp/src/TestApp.Infrastructure/Data/ProductRepository.cs").Should().BeTrue();
    fileSystem.FileExists("/test/TestApp/src/TestApp.Api/Controllers/ProductsController.cs").Should().BeTrue();
}
```

---

## 8. Performance Considerations

### 8.1 Lazy Template Loading

Templates are loaded at startup, even if not used. Consider lazy loading:

```csharp
public class LazyTemplateFactory : ITemplateFactory
{
    private readonly ConcurrentDictionary<string, Lazy<Template>> _templates = new();

    public Template GetTemplate(string name)
    {
        return _templates.GetOrAdd(name, n => new Lazy<Template>(() => LoadTemplate(n))).Value;
    }

    private Template LoadTemplate(string name)
    {
        // Load from embedded resource
        ...
    }
}
```

### 8.2 String Builder for Large Content

In `ModelAddService.UpdateServiceCollectionExtensionsAsync`, use `StringBuilder` for complex string manipulations:

```csharp
// Instead of multiple string.Insert() calls
var sb = new StringBuilder(content);
sb.Insert(insertPosition, registrationLine);
content = sb.ToString();
```

### 8.3 Avoid Multiple File Reads

When updating multiple aspects of a file, read once and write once:

```csharp
// Current: Multiple reads/writes
await UpdateServiceCollectionExtensionsAsync(...);  // Reads and writes
await UpdateAppRoutesAsync(...);                     // Reads and writes

// Better: Batch operations
var updates = new Dictionary<string, List<FileUpdate>>();
CollectServiceCollectionUpdates(updates, entity);
CollectAppRouteUpdates(updates, entity);
await ApplyAllUpdatesAsync(updates, cancellationToken);
```

---

## 9. Code Organization

### 9.1 Recommended Project Structure

```
src/DataBuilder.Cli/
├── Commands/
│   ├── Definitions/          # Command classes
│   ├── Handlers/             # Command handlers
│   └── Options/              # Shared option types
├── Common/
│   ├── Result.cs             # Result pattern
│   └── Extensions.cs         # Extension methods
├── Configuration/
│   ├── DataBuilderOptions.cs
│   └── Constants.cs          # All magic strings
├── Editors/
│   ├── IEditorStrategy.cs
│   └── Strategies/           # Editor implementations
├── Exceptions/
│   └── DataBuilderExceptions.cs
├── Generators/
│   ├── Angular/
│   ├── Api/
│   └── Pipeline/             # Generation pipeline
├── Models/
│   ├── EntityDefinition.cs
│   ├── PropertyDefinition.cs
│   └── SolutionOptions.cs
├── Services/
│   ├── Abstractions/         # Interfaces
│   └── Implementations/      # Implementations
├── Templates/
│   ├── ITemplateFactory.cs
│   ├── Angular/              # .sbn files
│   └── Api/                  # .sbn files
├── Utilities/
│   ├── NamingConventions.cs
│   ├── TypeMapper.cs
│   └── ProcessRunner.cs
└── Program.cs
```

### 9.2 Constants File

```csharp
// New file: src/DataBuilder.Cli/Configuration/Constants.cs
namespace DataBuilder.Cli.Configuration;

public static class Defaults
{
    public const string CouchbaseBucket = "general";
    public const string CouchbaseScope = "general";
    public const int AngularPort = 4200;
    public const int ApiPort = 5001;
}

public static class ProjectPaths
{
    public const string Src = "src";
    public const string Models = "Models";
    public const string Data = "Data";
    public const string Controllers = "Controllers";
    public const string Features = "features";
    public const string Services = "services";
}

public static class FileExtensions
{
    public const string CSharp = ".cs";
    public const string TypeScript = ".ts";
    public const string Html = ".html";
    public const string Scss = ".scss";
    public const string Json = ".json";
}

public static class RegexPatterns
{
    public const string RepositoryRegistration = @"services\.AddScoped<I\w+Repository, \w+Repository>\(\);";
    public const string RouteEntry = @"path:\s*'([^']+)'";
    public const string NavigationItem = @"<mat-nav-list>";
}
```

---

## 10. Implementation Roadmap

### Phase 1: Foundation (Week 1-2)
- [ ] Create `Result<T>` pattern classes
- [ ] Create custom exception hierarchy
- [ ] Create `Constants.cs` with all magic strings
- [ ] Add global exception handler

### Phase 2: Abstractions (Week 3-4)
- [ ] Implement `IFileSystem` abstraction
- [ ] Implement `ITemplateFactory` abstraction
- [ ] Implement editor strategy pattern
- [ ] Separate command definitions from handlers

### Phase 3: Services (Week 5-6)
- [ ] Split `ModelAddService` into focused services
- [ ] Implement generation pipeline pattern
- [ ] Add `SolutionOptionsBuilder`
- [ ] Add configuration file support

### Phase 4: Testing (Week 7-8)
- [ ] Create test infrastructure (`InMemoryFileSystem`, etc.)
- [ ] Add unit tests for all utilities
- [ ] Add integration tests for generators
- [ ] Add end-to-end CLI tests

### Phase 5: Documentation (Week 9)
- [ ] Add XML documentation to all public APIs
- [ ] Update README with architecture diagram
- [ ] Create developer guide for contributors

---

## Conclusion

The DataBuilder CLI codebase has a solid foundation with proper use of dependency injection, async patterns, and interface segregation. The main areas for improvement are:

1. **Error Handling** - Custom exceptions and Result pattern
2. **Code Organization** - Separate concerns into focused services
3. **Extensibility** - Pipeline and strategy patterns
4. **Testability** - File system abstraction and mocks
5. **Configuration** - Externalize settings and magic strings

Implementing these improvements will significantly enhance maintainability, testability, and developer experience while maintaining the existing functionality.

---

**Document Version:** 1.0  
**Last Updated:** February 2, 2026  
**Related:** [CODE-QUALITY-ANALYSIS.md](CODE-QUALITY-ANALYSIS.md)
