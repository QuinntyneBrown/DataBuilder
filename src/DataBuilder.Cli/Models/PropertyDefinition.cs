namespace DataBuilder.Cli.Models;

/// <summary>
/// Represents a property definition for an entity.
/// </summary>
public class PropertyDefinition
{
    /// <summary>
    /// The property name in PascalCase (e.g., "FirstName").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The property name in camelCase (e.g., "firstName").
    /// </summary>
    public string NameCamelCase { get; set; } = string.Empty;

    /// <summary>
    /// The C# type (e.g., "string", "int", "DateTime").
    /// </summary>
    public string CSharpType { get; set; } = string.Empty;

    /// <summary>
    /// The TypeScript type (e.g., "string", "number", "Date").
    /// </summary>
    public string TypeScriptType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this property is nullable.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Whether this property is required (non-nullable).
    /// </summary>
    public bool IsRequired => !IsNullable;

    /// <summary>
    /// Whether this property is a collection type.
    /// </summary>
    public bool IsCollection { get; set; }

    /// <summary>
    /// Whether this property is the entity's ID field (maps to Couchbase Meta.id()).
    /// </summary>
    public bool IsId { get; set; }

    /// <summary>
    /// Whether this property is an object/dictionary type.
    /// </summary>
    public bool IsObject { get; set; }

    /// <summary>
    /// The original JSON sample value (for documentation purposes).
    /// </summary>
    public string? SampleValue { get; set; }
}
