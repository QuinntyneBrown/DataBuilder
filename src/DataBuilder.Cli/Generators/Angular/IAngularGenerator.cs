using DataBuilder.Cli.Models;

namespace DataBuilder.Cli.Generators.Angular;

public record ComponentContent(string Ts, string Html, string Css);

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

    /// <summary>
    /// Generates an individual model file.
    /// </summary>
    Task<string> GenerateModelAsync(EntityDefinition entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an individual service file.
    /// </summary>
    Task<string> GenerateServiceAsync(EntityDefinition entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an individual list component (TS, HTML, CSS).
    /// </summary>
    Task<ComponentContent> GenerateListComponentAsync(EntityDefinition entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an individual detail component (TS, HTML, CSS).
    /// </summary>
    Task<ComponentContent> GenerateDetailComponentAsync(EntityDefinition entity, CancellationToken cancellationToken = default);
}
