using DataBuilder.Cli.Models;

namespace DataBuilder.Cli.Generators.Angular;

/// <summary>
/// Generates the Angular frontend project.
/// </summary>
public interface IAngularGenerator
{
    /// <summary>
    /// Generates the Angular project with all entities, services, and components.
    /// </summary>
    /// <param name="options">The solution options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task GenerateAsync(SolutionOptions options, CancellationToken cancellationToken = default);
}
