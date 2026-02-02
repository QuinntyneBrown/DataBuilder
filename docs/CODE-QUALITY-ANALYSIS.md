# Code Quality & Maintainability Analysis

**Project:** DataBuilder.Cli  
**Analysis Date:** February 2, 2026  
**Scope:** C# codebase in `src/DataBuilder.Cli`

## Executive Summary

This document outlines code quality issues, anti-patterns, and maintainability concerns identified in the DataBuilder CLI codebase. The analysis focuses on adherence to SOLID principles, code duplication, error handling, and documentation gaps.

**Priority Legend:**
- 🔴 **High** - Should be addressed soon; impacts reliability or maintainability
- 🟡 **Medium** - Should be addressed; impacts code quality
- 🟢 **Low** - Nice to have; minor improvements

---

## 1. Code Smells & Anti-Patterns

### 🔴 Generic Exception Handling
**Location:** `SolutionCreateCommand.cs:186-191`, `ModelAddCommand.cs:207-213`

**Issue:** Catching generic `Exception` without specific handling
```csharp
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}
```

**Recommendation:** 
- Create specific exception types (`SchemaParseException`, `TemplateGenerationException`)
- Handle known exceptions specifically
- Let unexpected exceptions bubble up with context

### 🟡 Magic Strings Throughout Codebase
**Locations:** Multiple files

**Examples:**
- `"general"` - Default Couchbase bucket/scope (SolutionCreateCommand:45, 50)
- `"notepad"`, `"nano"` - OS defaults (JsonEditorService:94, 103)
- `services\.AddScoped<I\w+Repository` - Regex patterns (ModelAddService:197)
- `"src"`, `"app"`, `"features"` - Path segments (ModelAddService:85, 92, 109)

**Recommendation:**
```csharp
public static class Defaults
{
    public const string CouchbaseBucket = "general";
    public const string CouchbaseScope = "general";
    public const string WindowsEditor = "notepad";
    public const string UnixEditor = "nano";
}

public static class ProjectPaths
{
    public const string Src = "src";
    public const string App = "app";
    public const string Features = "features";
}

public static class RegexPatterns
{
    public const string ServiceRegistration = @"services\.AddScoped<I\w+Repository";
    public const string ComponentRoute = @"path:\s*'([^']+)'";
}
```

### 🟡 God Object Pattern
**Location:** `SolutionOptions` class

**Issue:** Single class handles multiple concerns:
- Directory path calculations
- Project naming conventions
- Entity management
- Couchbase configuration

**Recommendation:** Split into focused types:
```csharp
public class SolutionOptions { ... }
public class ProjectStructure { ... }
public class CouchbaseConfiguration { ... }
public class EntityCollection { ... }
```

### 🔴 Duplicated Command Logic
**Locations:** `SolutionCreateCommandHandler`, `ModelAddCommandHandler`

**Issue:** Both handlers duplicate:
- JSON editor workflow (lines 101-122 vs 110-129)
- Sample JSON generation
- Error handling patterns
- Schema parsing

**Recommendation:** Extract to shared service:
```csharp
public interface IJsonWorkflowService
{
    Task<string> GetJsonInputAsync(string? jsonFile, CancellationToken cancellationToken);
}
```

---

## 2. Code Duplication

### 🔴 JSON Editor Flow (Critical)

**Duplicated in:**
- `SolutionCreateCommandHandler.cs:101-122`
- `ModelAddCommandHandler.cs:110-129`

**Duplicate Code:**
- Sample JSON structure generation
- File writing/reading
- Editor invocation
- Error messages
- Console output

**Impact:** ~44 lines duplicated; changes must be made in 2 places

**Solution:**
```csharp
public class JsonWorkflowService : IJsonWorkflowService
{
    private readonly IJsonEditorService _editorService;
    
    public async Task<string> GetJsonInputAsync(string? jsonFile, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(jsonFile))
        {
            return await File.ReadAllTextAsync(jsonFile, cancellationToken);
        }
        
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, GenerateSampleJson(), cancellationToken);
        await _editorService.OpenEditorAsync(tempFile, cancellationToken);
        return await File.ReadAllTextAsync(tempFile, cancellationToken);
    }
    
    private string GenerateSampleJson() { /* ... */ }
}
```

### 🔴 Validation Pattern Duplication

**Location:** `ModelAddCommand.cs:232-296`

**Issue:** Validation logic is command-specific, should be reusable

**Recommendation:** Extract to service with shared types:
```csharp
public class ValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}

public interface IProjectValidator
{
    ValidationResult ValidateProjectStructure(string currentDirectory);
}
```

### 🟡 Repository Registration Logic

**Location:** `ModelAddService.cs:119-161`

**Issue:** Brittle regex-based file manipulation repeated for different file types

**Recommendation:** Create abstraction:
```csharp
public interface IProjectFileUpdater
{
    Task AddServiceRegistrationAsync(string filePath, string registration);
    Task AddComponentRouteAsync(string filePath, string route);
    Task AddNavigationItemAsync(string filePath, NavigationItem item);
}
```

---

## 3. Hard-Coded Values

| Category | Value | Location | Impact |
|----------|-------|----------|--------|
| **Defaults** | `"general"` | SolutionCreateCommand:45, 50 | Bucket/scope defaults |
| **Template Names** | Array of strings | ApiGenerator:26-30, AngularGenerator:29-35 | Template loading fragile |
| **Editors** | `"notepad"`, `"nano"` | JsonEditorService:94, 103 | OS detection hardcoded |
| **Paths** | `"src"`, `"app"`, `"features"` | ModelAddService:85, 92, 109 | Project structure assumptions |
| **Regex** | Multiple patterns | ModelAddService:135, 148, 197 | File parsing brittle |
| **File Extensions** | `-list.component.ts/.html/.scss` | ModelAddService:103-105 | Angular conventions |

### Recommendations

**Create Constants File:**
```csharp
namespace DataBuilder.Cli.Constants;

public static class AngularConventions
{
    public const string ComponentSuffix = ".component.ts";
    public const string TemplateSuffix = ".component.html";
    public const string StyleSuffix = ".component.scss";
    public const string ListSuffix = "-list";
    public const string EditSuffix = "-edit";
}

public static class TemplateNames
{
    public static readonly string[] Api = new[]
    {
        "Entity", "controller", "repository", "Request", 
        "PagedResult", "ServiceCollectionExtensions", 
        "Program", "appsettings", "csproj", 
        "Core.csproj", "Infrastructure.csproj"
    };
    
    public static readonly string[] Angular = new[]
    {
        "list.component", "edit.component", "models",
        "service", "module", "routing", "material.module",
        "angular.json", "tsconfig", "package.json"
    };
}
```

---

## 4. Missing Error Handling

### 🔴 Critical Issues

#### Template Loading Failures
**Location:** `ApiGenerator.cs:20`, `AngularGenerator.cs:23`

**Issue:** `LoadTemplates()` logs warning but continues; later compilation will fail
```csharp
if (stream == null)
{
    _logger.LogWarning("Template not found: {ResourceName}", resourceName);
    continue; // ⚠️ Should throw exception
}
```

**Fix:**
```csharp
if (stream == null)
{
    throw new TemplateNotFoundException($"Required template not found: {resourceName}");
}
```

#### Process Timeout
**Location:** `JsonEditorService.cs:43-49`

**Issue:** `WaitForExitAsync` can hang indefinitely
```csharp
await process.WaitForExitAsync(cancellationToken); // No timeout
```

**Fix:**
```csharp
using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
cts.CancelAfter(TimeSpan.FromMinutes(30)); // 30-minute timeout
await process.WaitForExitAsync(cts.Token);
```

#### Regex Match Validation
**Location:** `ModelAddService.cs:198`

**Issue:** Assumes regex match succeeds
```csharp
var match = Regex.Match(content, pattern);
content = content.Insert(match.Index, registration); // ⚠️ No null check
```

**Fix:**
```csharp
var match = Regex.Match(content, pattern);
if (!match.Success)
{
    throw new InvalidOperationException($"Could not find injection point in {filePath}");
}
content = content.Insert(match.Index, registration);
```

### 🟡 Medium Priority

| Location | Issue | Recommendation |
|----------|-------|----------------|
| `ModelAddService:123-127` | File existence check but no content validation | Validate JSON structure |
| `SolutionCreateCommand:175` | No validation after generation | Check generated files exist |
| `AngularGenerator:115` | Assumes `angular.json` exists | Add existence check |

---

## 5. Missing XML Documentation

### Public APIs Without Documentation

**Services:**
- ❌ `IModelAddService` interface (entire interface)
- ❌ `ModelAddService` class
- ⚠️ `ISolutionGenerator.GenerateAsync` (has doc)
- ❌ `ISchemaParser.ParseSchema` (missing)

**Generators:**
- ❌ `ApiGenerator.GenerateCoreProjectAsync`
- ❌ `ApiGenerator.GenerateInfrastructureProjectAsync`
- ❌ `ApiGenerator.GenerateApiProjectAsync`
- ❌ `AngularGenerator.CreateAngularProjectAsync`
- ❌ `AngularGenerator.ConfigureMonacoAssetsAsync`
- ❌ `AngularGenerator.GenerateComponentsAsync`

**Models:**
- ⚠️ `EntityDefinition` (partial docs)
- ⚠️ `PropertyDefinition` (partial docs)
- ❌ `SolutionOptions` properties

**Utilities:**
- ❌ `TypeMapper` (all methods)
- ❌ `NamingConventions` (all methods)
- ⚠️ `ProcessRunner` (minimal docs)

### Recommendation

Add comprehensive XML documentation:
```csharp
/// <summary>
/// Adds a new entity model to an existing DataBuilder solution.
/// </summary>
/// <param name="jsonFilePath">Path to JSON schema file, or null to open editor.</param>
/// <param name="bucket">Couchbase bucket name.</param>
/// <param name="scope">Couchbase scope name.</param>
/// <param name="collection">Optional collection name override.</param>
/// <param name="useTypeDiscriminator">Whether to use type field for entity discrimination.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Exit code (0 for success).</returns>
/// <exception cref="InvalidOperationException">Thrown when not in a valid DataBuilder solution.</exception>
public async Task<int> AddModelAsync(
    string? jsonFilePath,
    string bucket,
    string scope,
    string? collection,
    bool useTypeDiscriminator,
    CancellationToken cancellationToken)
{
    // ...
}
```

---

## 6. Inconsistent Naming & Patterns

### Inconsistencies Found

| Pattern | Issue | Example |
|---------|-------|---------|
| **Method Naming** | Mix of verbose/terse async names | `GenerateAsync` vs `GenerateCoreProjectAsync` |
| **Logging Levels** | Overuse of `LogInformation` | Should use `LogDebug` for verbose operations |
| **Directory Creation** | Inconsistent safety checks | Sometimes checks exist, sometimes doesn't |
| **Path Construction** | Mix of `Path.Combine()` and interpolation | Inconsistent across files |
| **Casing** | Template names inconsistent | `"Entity"` vs `"controller"` (Pascal vs camel) |

### Recommendations

**Establish Conventions:**

```csharp
// ✅ Consistent async naming
Task GenerateAsync(...)           // Top-level
Task GenerateCoreAsync(...)       // Sub-operations
Task GenerateInfraAsync(...)      

// ✅ Consistent logging
_logger.LogDebug("Loading template: {Name}", name);        // Verbose
_logger.LogInformation("Generated {Count} files", count);  // Progress
_logger.LogWarning("Template not found: {Name}", name);    // Issues

// ✅ Always use Path.Combine
var path = Path.Combine(baseDir, "src", projectName);  // ✅
var path = $"{baseDir}\\src\\{projectName}";           // ❌

// ✅ Consistent template naming (all PascalCase)
"Entity", "Controller", "Repository", "Request"
```

---

## 7. SOLID Principle Violations

### 🔴 Single Responsibility Principle

#### `ModelAddService` - Multiple Responsibilities
**Location:** `ModelAddService.cs:30-242`

**Violations:**
1. Generates API code (Core, Infrastructure, Api projects)
2. Generates Angular code (components, services)
3. Updates existing project files (DI registration)
4. Performs file system operations
5. Parses and validates project structure

**Solution:**
```csharp
// Split into focused services
public class ModelAddService : IModelAddService
{
    private readonly IApiModelGenerator _apiGenerator;
    private readonly IAngularModelGenerator _angularGenerator;
    private readonly IProjectFileUpdater _fileUpdater;
    private readonly IProjectValidator _validator;
    
    public async Task AddModelAsync(...)
    {
        var validation = _validator.ValidateProjectStructure(directory);
        if (!validation.IsValid) { /* ... */ }
        
        await _apiGenerator.GenerateModelAsync(entity, ...);
        await _angularGenerator.GenerateModelAsync(entity, ...);
        await _fileUpdater.UpdateProjectFilesAsync(entity, ...);
    }
}
```

#### `ModelAddCommandHandler` - Too Many Concerns
**Location:** `ModelAddCommand.cs:68-214`

**Violations:**
1. Command-line parsing
2. Validation logic
3. JSON editing workflow
4. Schema parsing
5. Service orchestration

**Solution:** Extract validation and workflow to services (see section 2)

### 🟡 Open/Closed Principle

#### Template Loading - Requires Code Changes
**Location:** `ApiGenerator.cs:26-30`, `AngularGenerator.cs:29-35`

**Issue:** Adding new templates requires modifying array in code

**Solution:**
```csharp
public interface ITemplateLoader
{
    Template LoadTemplate(string name);
    IEnumerable<Template> LoadAllTemplates(string category);
}

public class EmbeddedTemplateLoader : ITemplateLoader
{
    public Template LoadTemplate(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames()
            .Where(r => r.Contains(name));
        // Auto-discover templates
    }
}
```

#### Editor Selection - Hardcoded
**Location:** `JsonEditorService.cs:75-104`

**Issue:** Adding new editors requires code changes

**Solution:**
```csharp
public interface IEditorProvider
{
    string GetEditorCommand();
    bool IsAvailable();
}

public class EditorSelector
{
    private readonly IEditorProvider[] _providers;
    
    public IEditorProvider SelectEditor()
    {
        return _providers.FirstOrDefault(p => p.IsAvailable()) 
            ?? throw new NoEditorException();
    }
}
```

### 🟢 Liskov Substitution Principle

#### Inner Class Breaks Substitution
**Location:** `ModelAddCommand.cs:285-296`

**Issue:** `ValidationResult` is command-specific inner class

**Solution:** Move to shared Models namespace for reuse

### 🟡 Interface Segregation Principle

#### Command Options Expose Too Much
**Location:** `SolutionCreateCommand.cs:14-20`

**Issue:** All options exposed as public properties; clients must know all details

**Solution:** Group related options:
```csharp
public class CouchbaseOptions
{
    public string Bucket { get; init; }
    public string Scope { get; init; }
    public string? Collection { get; init; }
    public bool UseTypeDiscriminator { get; init; }
}

public class SolutionCreateCommand
{
    public Option<string> NameOption { get; }
    public Option<string> DirectoryOption { get; }
    public Option<CouchbaseOptions> CouchbaseOption { get; }
}
```

### 🟡 Dependency Inversion Principle

**Good Examples:**
- ✅ `AngularGenerator` depends on `IProcessRunner` abstraction
- ✅ Services registered via interfaces in DI container

**Issues:**
- ❌ Template loading is internal logic, not injectable (hard to test)
- ❌ File system operations use `File.` and `Directory.` directly (hard to mock)

**Recommendation:**
```csharp
public interface IFileSystem
{
    Task<string> ReadAllTextAsync(string path, CancellationToken ct);
    Task WriteAllTextAsync(string path, string content, CancellationToken ct);
    bool FileExists(string path);
    void CreateDirectory(string path);
}

// Mock for testing
public class TestFileSystem : IFileSystem { ... }
```

---

## 8. Testing Considerations

### Testability Issues

1. **Static File System Calls** - Hard to unit test without file system
2. **Template Loading** - Cannot inject mock templates
3. **Process Execution** - Hard to test without running actual processes
4. **Hardcoded Paths** - Tests must match exact directory structure

### Recommendations

```csharp
// Make file operations testable
public class ApiGenerator
{
    private readonly IFileSystem _fileSystem;
    private readonly ITemplateLoader _templateLoader;
    
    public ApiGenerator(IFileSystem fileSystem, ITemplateLoader templateLoader)
    {
        _fileSystem = fileSystem;
        _templateLoader = templateLoader;
    }
}

// Example test
[Fact]
public async Task GenerateAsync_CreatesExpectedFiles()
{
    var fileSystem = new InMemoryFileSystem();
    var templateLoader = new MockTemplateLoader();
    var generator = new ApiGenerator(fileSystem, templateLoader);
    
    await generator.GenerateAsync(options);
    
    Assert.True(fileSystem.FileExists("src/Core/Models/Product.cs"));
}
```

---

## 9. Recommended Refactoring Plan

### Phase 1: Critical Fixes (Week 1)
1. ✅ Extract `IJsonWorkflowService` to eliminate duplication
2. ✅ Add timeouts to `Process.WaitForExitAsync()`
3. ✅ Validate regex matches before using
4. ✅ Create shared `ValidationResult` type
5. ✅ Add missing null checks

### Phase 2: Configuration & Constants (Week 2)
1. ✅ Create `Constants` folder with static classes
2. ✅ Move all magic strings to constants
3. ✅ Extract regex patterns
4. ✅ Externalize default values

### Phase 3: Service Extraction (Week 3)
1. ✅ Split `ModelAddService` into focused services
2. ✅ Create `IProjectFileUpdater` service
3. ✅ Create `ITemplateLoader` abstraction
4. ✅ Extract validation logic to `IProjectValidator`

### Phase 4: Documentation (Week 4)
1. ✅ Add XML docs to all public APIs
2. ✅ Document exceptions thrown
3. ✅ Add usage examples
4. ✅ Update README with architecture diagram

### Phase 5: Testability (Optional)
1. ✅ Introduce `IFileSystem` abstraction
2. ✅ Make template loading injectable
3. ✅ Add integration tests
4. ✅ Add unit tests with mocks

---

## 10. Metrics

### Code Quality Scores

| Metric | Current | Target | Gap |
|--------|---------|--------|-----|
| **Code Coverage** | Unknown | 80% | TBD |
| **Cyclomatic Complexity** | High (ModelAddService) | <10 per method | Needs refactoring |
| **Code Duplication** | ~8% (44 lines) | <3% | Extract services |
| **XML Documentation** | ~30% | 100% | Add docs |
| **Magic Strings** | 20+ occurrences | 0 | Extract constants |

### Technical Debt Estimate

| Category | Effort (hours) | Priority |
|----------|---------------|----------|
| Extract duplicated code | 4-6 | High |
| Add error handling | 3-4 | High |
| Create constants | 2-3 | Medium |
| Split services (SRP) | 8-12 | High |
| Add XML documentation | 6-8 | Medium |
| Testability improvements | 8-10 | Low |
| **Total** | **31-43** | - |

---

## Conclusion

The DataBuilder CLI is functionally complete but has maintainability concerns that will compound as the codebase grows. The highest-priority improvements are:

1. **Eliminate code duplication** (JSON workflow, validation)
2. **Improve error handling** (timeouts, validation)
3. **Extract constants** (magic strings)
4. **Split responsibilities** (ModelAddService, command handlers)
5. **Add documentation** (XML docs for public APIs)

Addressing these issues will significantly improve code quality, testability, and long-term maintainability while reducing the risk of bugs and making the codebase easier for contributors to understand.

---

**Document Version:** 1.0  
**Last Updated:** February 2, 2026  
**Maintainer:** Development Team
