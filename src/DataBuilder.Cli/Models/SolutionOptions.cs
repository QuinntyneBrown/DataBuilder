using DataBuilder.Cli.Utilities;

namespace DataBuilder.Cli.Models;

/// <summary>
/// Options for generating a solution.
/// </summary>
public class SolutionOptions
{
    /// <summary>
    /// The solution name (e.g., "my-app").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The solution name in PascalCase (e.g., "MyApp").
    /// </summary>
    public string NamePascalCase => NamingConventions.ToPascalCase(Name.Replace("-", " ").Replace("_", " "));

    /// <summary>
    /// The solution name in kebab-case (e.g., "my-app").
    /// </summary>
    public string NameKebabCase => NamingConventions.ToKebabCase(Name);

    /// <summary>
    /// The target directory where the solution will be created.
    /// </summary>
    public string Directory { get; set; } = string.Empty;

    /// <summary>
    /// The full path to the solution directory.
    /// </summary>
    public string SolutionDirectory => Path.Combine(Directory, NameKebabCase);

    /// <summary>
    /// The full path to the src directory.
    /// </summary>
    public string SrcDirectory => Path.Combine(SolutionDirectory, "src");

    /// <summary>
    /// The Core project name (e.g., "MyApp.Core").
    /// </summary>
    public string CoreProjectName => $"{NamePascalCase}.Core";

    /// <summary>
    /// The Infrastructure project name (e.g., "MyApp.Infrastructure").
    /// </summary>
    public string InfrastructureProjectName => $"{NamePascalCase}.Infrastructure";

    /// <summary>
    /// The API project name (e.g., "MyApp.Api").
    /// </summary>
    public string ApiProjectName => $"{NamePascalCase}.Api";

    /// <summary>
    /// The UI project name (e.g., "MyApp.Ui").
    /// </summary>
    public string UiProjectName => $"{NamePascalCase}.Ui";

    /// <summary>
    /// The full path to the Core project directory.
    /// </summary>
    public string CoreProjectDirectory => Path.Combine(SrcDirectory, CoreProjectName);

    /// <summary>
    /// The full path to the Infrastructure project directory.
    /// </summary>
    public string InfrastructureProjectDirectory => Path.Combine(SrcDirectory, InfrastructureProjectName);

    /// <summary>
    /// The full path to the API project directory.
    /// </summary>
    public string ApiProjectDirectory => Path.Combine(SrcDirectory, ApiProjectName);

    /// <summary>
    /// The full path to the UI project directory.
    /// </summary>
    public string UiProjectDirectory => Path.Combine(SrcDirectory, UiProjectName);

    /// <summary>
    /// The entities to generate.
    /// </summary>
    public List<EntityDefinition> Entities { get; set; } = new();

    /// <summary>
    /// The Couchbase bucket name.
    /// </summary>
    public string CouchbaseBucket => NamePascalCase;

    /// <summary>
    /// The API port number.
    /// </summary>
    public int ApiPort { get; set; } = 5000;

    /// <summary>
    /// The Angular app port number.
    /// </summary>
    public int AngularPort { get; set; } = 4200;
}
