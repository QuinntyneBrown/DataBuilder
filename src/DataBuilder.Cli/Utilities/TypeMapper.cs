using System.Text.Json;

namespace DataBuilder.Cli.Utilities;

/// <summary>
/// Maps JSON types to C# and TypeScript types.
/// </summary>
public static class TypeMapper
{
    /// <summary>
    /// Maps a JSON value to its corresponding C# type.
    /// </summary>
    public static string ToCSharpType(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => InferStringType(element.GetString()),
            JsonValueKind.Number => InferNumberType(element),
            JsonValueKind.True or JsonValueKind.False => "bool",
            JsonValueKind.Array => InferArrayType(element),
            JsonValueKind.Object => "Dictionary<string, object>",
            JsonValueKind.Null => "string?",
            _ => "object"
        };
    }

    /// <summary>
    /// Maps a JSON value to its corresponding TypeScript type.
    /// </summary>
    public static string ToTypeScriptType(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => InferTypeScriptStringType(element.GetString()),
            JsonValueKind.Number => "number",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Array => InferTypeScriptArrayType(element),
            JsonValueKind.Object => "Record<string, any>",
            JsonValueKind.Null => "string | null",
            _ => "any"
        };
    }

    /// <summary>
    /// Maps a C# type name to its TypeScript equivalent.
    /// </summary>
    public static string CSharpToTypeScript(string csharpType)
    {
        // Remove nullable indicator for mapping
        var baseType = csharpType.TrimEnd('?');
        var isNullable = csharpType.EndsWith('?');

        var tsType = baseType switch
        {
            "string" => "string",
            "int" => "number",
            "long" => "number",
            "double" => "number",
            "decimal" => "number",
            "float" => "number",
            "bool" => "boolean",
            "DateTime" => "Date",
            "DateOnly" => "Date",
            "TimeOnly" => "string",
            "Guid" => "string",
            "object" => "any",
            "Dictionary<string, object>" => "Record<string, any>",
            _ when baseType.StartsWith("List<") => ExtractListType(baseType) + "[]",
            _ when baseType.EndsWith("[]") => CSharpToTypeScript(baseType[..^2]) + "[]",
            _ => "any"
        };

        return isNullable ? $"{tsType} | null" : tsType;
    }

    private static string InferStringType(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "string";

        // Check if it looks like a GUID
        if (Guid.TryParse(value, out _))
            return "Guid";

        // Check if it looks like a DateTime
        if (DateTime.TryParse(value, out _))
            return "DateTime";

        return "string";
    }

    private static string InferTypeScriptStringType(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "string";

        // Check if it looks like a DateTime
        if (DateTime.TryParse(value, out _))
            return "Date";

        return "string";
    }

    private static string InferNumberType(JsonElement element)
    {
        // Try to get as different number types to determine precision
        if (element.TryGetInt32(out _))
            return "int";

        if (element.TryGetInt64(out _))
            return "long";

        if (element.TryGetDouble(out var d))
        {
            // Check if it has decimal places
            if (d % 1 != 0)
                return "decimal";
        }

        return "int";
    }

    private static string InferArrayType(JsonElement element)
    {
        if (element.GetArrayLength() == 0)
            return "List<object>";

        var firstElement = element.EnumerateArray().First();
        var elementType = ToCSharpType(firstElement);

        return $"List<{elementType}>";
    }

    private static string InferTypeScriptArrayType(JsonElement element)
    {
        if (element.GetArrayLength() == 0)
            return "any[]";

        var firstElement = element.EnumerateArray().First();
        var elementType = ToTypeScriptType(firstElement);

        return $"{elementType}[]";
    }

    private static string ExtractListType(string listType)
    {
        // Extract type from List<T>
        var start = listType.IndexOf('<') + 1;
        var end = listType.LastIndexOf('>');
        if (start > 0 && end > start)
        {
            var innerType = listType[start..end];
            return CSharpToTypeScript(innerType);
        }
        return "any";
    }
}
