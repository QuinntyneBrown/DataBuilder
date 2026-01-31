using DataBuilder.Cli.Models;

namespace DataBuilder.Cli.Services;

/// <summary>
/// Parses JSON schema to entity definitions.
/// </summary>
public interface ISchemaParser
{
    /// <summary>
    /// Parses a JSON string to extract entity definitions.
    /// The JSON object's property names become entity names,
    /// and the property values (objects) define the entity properties.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>A list of entity definitions.</returns>
    List<EntityDefinition> Parse(string json);
}
