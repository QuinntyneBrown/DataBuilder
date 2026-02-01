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
    /// Whether this property is an array type.
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// Whether this property is a complex type (object or array) that requires a JSON editor.
    /// </summary>
    public bool IsComplexType => IsObject || IsArray;

    /// <summary>
    /// Whether this property is a datetime field that should use date/time pickers.
    /// Detected based on naming conventions:
    /// - Properties ending with "At" containing a past tense verb (e.g., "createdAt", "updatedAt")
    /// - Properties ending with "DateTime" (e.g., "startDateTime", "endDateTime")
    /// </summary>
    public bool IsDateTime => IsDateTimeProperty(Name);

    /// <summary>
    /// The original JSON sample value (for documentation purposes).
    /// </summary>
    public string? SampleValue { get; set; }

    /// <summary>
    /// Determines if a property name indicates a datetime field based on naming conventions.
    /// </summary>
    private static bool IsDateTimeProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return false;

        // Check for "DateTime" suffix (e.g., "startDateTime", "endDateTime")
        if (propertyName.EndsWith("DateTime", StringComparison.OrdinalIgnoreCase))
            return true;

        // Check for "At" suffix with past tense verb patterns
        if (propertyName.EndsWith("At", StringComparison.OrdinalIgnoreCase))
        {
            // Common past tense verb patterns that precede "At"
            var pastTensePatterns = new[]
            {
                "created", "updated", "modified", "deleted", "published",
                "submitted", "approved", "rejected", "completed", "started",
                "ended", "expired", "activated", "deactivated", "logged",
                "registered", "verified", "confirmed", "cancelled", "processed",
                "sent", "received", "opened", "closed", "archived", "restored",
                "synced", "synchronized", "indexed", "refreshed", "loaded",
                "saved", "edited", "viewed", "accessed", "downloaded", "uploaded",
                "installed", "uninstalled", "deployed", "released", "launched",
                "scheduled", "triggered", "executed", "finished", "failed",
                "succeeded", "generated", "imported", "exported", "migrated",
                "lastModified", "lastUpdated", "lastAccessed", "lastLogin", "lastSeen"
            };

            var lowerName = propertyName.ToLowerInvariant();
            foreach (var pattern in pastTensePatterns)
            {
                if (lowerName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) ||
                    lowerName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
