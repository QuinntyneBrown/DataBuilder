using DataBuilder.Cli.Models;

namespace DataBuilder.Cli.Services;

/// <summary>
/// Orchestrates the generation of a complete solution.
/// </summary>
public interface ISolutionGenerator
{
    /// <summary>
    /// Generates a complete solution with API and Angular projects.
    /// </summary>
    /// <param name="options">The solution options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task GenerateAsync(SolutionOptions options, CancellationToken cancellationToken = default);
}
