using DataBuilder.Cli.Utilities;

namespace DataBuilder.Cli.Models;

/// <summary>
/// Represents an entity definition parsed from JSON.
/// </summary>
public class EntityDefinition
{
    /// <summary>
    /// The entity name in PascalCase (e.g., "ToDo").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The entity name in camelCase (e.g., "toDo").
    /// </summary>
    public string NameCamelCase => NamingConventions.ToCamelCase(Name);

    /// <summary>
    /// The entity name in kebab-case (e.g., "to-do").
    /// </summary>
    public string NameKebabCase => NamingConventions.ToKebabCase(Name);

    /// <summary>
    /// The pluralized entity name in PascalCase (e.g., "ToDos").
    /// </summary>
    public string NamePlural => NamingConventions.Pluralize(Name);

    /// <summary>
    /// The pluralized entity name in camelCase (e.g., "toDos").
    /// </summary>
    public string NamePluralCamelCase => NamingConventions.ToCamelCase(NamePlural);

    /// <summary>
    /// The pluralized entity name in kebab-case (e.g., "to-dos").
    /// </summary>
    public string NamePluralKebabCase => NamingConventions.ToKebabCase(NamePlural);

    /// <summary>
    /// Human-readable display name (e.g., "To Do").
    /// </summary>
    public string DisplayName => NamingConventions.ToTitleCase(Name);

    /// <summary>
    /// Human-readable pluralized display name (e.g., "To Dos").
    /// </summary>
    public string DisplayNamePlural => NamingConventions.ToTitleCase(NamePlural);

    /// <summary>
    /// The properties of this entity.
    /// </summary>
    public List<PropertyDefinition> Properties { get; set; } = new();

    /// <summary>
    /// The ID property for this entity (maps to Couchbase Meta.id()).
    /// </summary>
    public PropertyDefinition? IdProperty => Properties.FirstOrDefault(p => p.IsId);

    /// <summary>
    /// The ID property name in PascalCase (e.g., "ProductId" or "Id").
    /// </summary>
    public string IdPropertyName => IdProperty?.Name ?? "Id";

    /// <summary>
    /// The ID property name in camelCase (e.g., "productId" or "id").
    /// </summary>
    public string IdPropertyNameCamelCase => IdProperty?.NameCamelCase ?? "id";

    /// <summary>
    /// Non-ID properties (for create/update requests that don't include the ID).
    /// </summary>
    public List<PropertyDefinition> NonIdProperties => Properties.Where(p => !p.IsId).ToList();
}
