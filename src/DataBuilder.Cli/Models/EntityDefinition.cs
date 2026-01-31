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

    /// <summary>
    /// Properties suitable for list display (max 5 columns).
    /// Only includes: id, version, versionNumber, name, description (if they exist).
    /// </summary>
    public List<PropertyDefinition> ListDisplayProperties
    {
        get
        {
            var allowedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "id", "version", "versionnumber", "name", "description"
            };

            return Properties
                .Where(p => allowedNames.Contains(p.Name))
                .Take(5)
                .ToList();
        }
    }

    /// <summary>
    /// Whether the entity has a name property for display.
    /// </summary>
    public bool HasNameProperty => Properties.Any(p =>
        p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Whether the entity has a description property for display.
    /// </summary>
    public bool HasDescriptionProperty => Properties.Any(p =>
        p.Name.Equals("Description", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Material icon name for this entity in the UI navigation.
    /// </summary>
    public string Icon => GetDefaultIcon();

    /// <summary>
    /// Whether to use a type discriminator field in documents.
    /// When false (default), each entity is stored in its own collection.
    /// </summary>
    public bool UseTypeDiscriminator { get; set; } = false;

    /// <summary>
    /// The Couchbase bucket name for this entity.
    /// </summary>
    public string Bucket { get; set; } = "general";

    /// <summary>
    /// The Couchbase scope name for this entity.
    /// </summary>
    public string Scope { get; set; } = "general";

    /// <summary>
    /// The Couchbase collection name for this entity.
    /// When UseTypeDiscriminator is false, defaults to the entity name in camelCase.
    /// </summary>
    public string? CollectionOverride { get; set; }

    /// <summary>
    /// The effective collection name for this entity.
    /// </summary>
    public string Collection => CollectionOverride ?? (UseTypeDiscriminator ? "general" : NameCamelCase);

    private string GetDefaultIcon()
    {
        var nameLower = Name.ToLowerInvariant();

        return nameLower switch
        {
            "user" or "users" => "person",
            "account" or "accounts" => "account_circle",
            "category" or "categories" => "category",
            "product" or "products" => "inventory_2",
            "order" or "orders" => "shopping_cart",
            "idea" or "ideas" => "lightbulb",
            "task" or "tasks" => "task_alt",
            "project" or "projects" => "folder",
            "document" or "documents" => "description",
            "setting" or "settings" => "settings",
            "role" or "roles" => "admin_panel_settings",
            "permission" or "permissions" => "security",
            "notification" or "notifications" => "notifications",
            "message" or "messages" => "message",
            "comment" or "comments" => "comment",
            "report" or "reports" => "assessment",
            "dashboard" => "dashboard",
            "log" or "logs" => "history",
            "file" or "files" => "attach_file",
            "image" or "images" => "image",
            "video" or "videos" => "videocam",
            "event" or "events" => "event",
            "calendar" => "calendar_today",
            "contact" or "contacts" => "contacts",
            "customer" or "customers" => "people",
            "employee" or "employees" => "badge",
            "team" or "teams" => "groups",
            _ => "list"
        };
    }
}
