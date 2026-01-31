using DataBuilder.Cli.Models;

namespace DataBuilder.Cli.Generators.Api;

/// <summary>
/// Generates the C# N-Tier API project.
/// </summary>
public interface IApiGenerator
{
    /// <summary>
    /// Generates the API project with all entities, controllers, and services.
    /// </summary>
    /// <param name="options">The solution options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task GenerateAsync(SolutionOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an individual entity file.
    /// </summary>
    Task<string> GenerateEntityAsync(SolutionOptions options, EntityDefinition entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an individual repository file.
    /// </summary>
    Task<string> GenerateRepositoryAsync(SolutionOptions options, EntityDefinition entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an individual controller file.
    /// </summary>
    Task<string> GenerateControllerAsync(SolutionOptions options, EntityDefinition entity, CancellationToken cancellationToken = default);
}
