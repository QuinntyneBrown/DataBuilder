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
}
