using System.Text.Json;
using DataBuilder.Cli.Models;
using DataBuilder.Cli.Utilities;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli.Services;

/// <summary>
/// Parses JSON schema to entity definitions.
/// </summary>
public class SchemaParser : ISchemaParser
{
    private readonly ILogger<SchemaParser> _logger;

    public SchemaParser(ILogger<SchemaParser> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public List<EntityDefinition> Parse(string json)
    {
        _logger.LogDebug("Parsing JSON schema");

        var entities = new List<EntityDefinition>();

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("JSON root must be an object where each property represents an entity.");
            }

            foreach (var entityProperty in root.EnumerateObject())
            {
                var entity = ParseEntity(entityProperty.Name, entityProperty.Value);
                entities.Add(entity);
                _logger.LogInformation("Parsed entity: {EntityName} with {PropertyCount} properties",
                    entity.Name, entity.Properties.Count);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON schema");
            throw new InvalidOperationException($"Invalid JSON: {ex.Message}", ex);
        }

        return entities;
    }

    private EntityDefinition ParseEntity(string propertyName, JsonElement element)
    {
        // Convert camelCase property name to PascalCase entity name
        var entityName = NamingConventions.ToPascalCase(propertyName);

        var entity = new EntityDefinition
        {
            Name = entityName,
            Properties = new List<PropertyDefinition>()
        };

        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException($"Entity '{propertyName}' must be defined as a JSON object.");
        }

        foreach (var prop in element.EnumerateObject())
        {
            var property = ParseProperty(prop.Name, prop.Value);
            entity.Properties.Add(property);
        }

        // Detect or add ID property
        EnsureIdProperty(entity);

        return entity;
    }

    private void EnsureIdProperty(EntityDefinition entity)
    {
        // Check for {entityName}Id (e.g., "ProductId" for "Product")
        var entityIdName = $"{entity.Name}Id";
        var entityIdProp = entity.Properties.FirstOrDefault(p =>
            string.Equals(p.Name, entityIdName, StringComparison.OrdinalIgnoreCase));

        if (entityIdProp != null)
        {
            entityIdProp.IsId = true;
            // Ensure ID is string type for Couchbase Meta.id() compatibility
            entityIdProp.CSharpType = "string";
            entityIdProp.TypeScriptType = "string";
            _logger.LogDebug("Using {PropertyName} as ID property for entity {EntityName}", entityIdProp.Name, entity.Name);
            return;
        }

        // Check for "Id" property
        var idProp = entity.Properties.FirstOrDefault(p =>
            string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase));

        if (idProp != null)
        {
            idProp.IsId = true;
            // Ensure ID is string type for Couchbase Meta.id() compatibility
            idProp.CSharpType = "string";
            idProp.TypeScriptType = "string";
            _logger.LogDebug("Using {PropertyName} as ID property for entity {EntityName}", idProp.Name, entity.Name);
            return;
        }

        // No ID property found, add one
        var newIdProp = new PropertyDefinition
        {
            Name = "Id",
            NameCamelCase = "id",
            CSharpType = "string",
            TypeScriptType = "string",
            IsNullable = false,
            IsCollection = false,
            IsId = true
        };

        // Insert at the beginning of properties
        entity.Properties.Insert(0, newIdProp);
        _logger.LogInformation("Added Id property to entity {EntityName} (maps to Couchbase Meta.id())", entity.Name);
    }

    private PropertyDefinition ParseProperty(string propertyName, JsonElement element)
    {
        var pascalName = NamingConventions.ToPascalCase(propertyName);
        var camelName = NamingConventions.ToCamelCase(propertyName);
        var csharpType = TypeMapper.ToCSharpType(element);
        var tsType = TypeMapper.ToTypeScriptType(element);

        var isNullable = element.ValueKind == JsonValueKind.Null;
        var isCollection = csharpType.StartsWith("List<") || csharpType.EndsWith("[]");

        string? sampleValue = element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };

        return new PropertyDefinition
        {
            Name = pascalName,
            NameCamelCase = camelName,
            CSharpType = csharpType,
            TypeScriptType = tsType,
            IsNullable = isNullable,
            IsCollection = isCollection,
            SampleValue = sampleValue
        };
    }
}
