using Humanizer;

namespace DataBuilder.Cli.Utilities;

/// <summary>
/// Provides naming convention transformations for code generation.
/// </summary>
public static class NamingConventions
{
    /// <summary>
    /// Converts a string to PascalCase.
    /// Examples: "to_do" -> "ToDo", "to-do" -> "ToDo", "toDo" -> "ToDo"
    /// </summary>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Pascalize();
    }

    /// <summary>
    /// Converts a string to camelCase.
    /// Examples: "ToDo" -> "toDo", "to_do" -> "toDo"
    /// </summary>
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Camelize();
    }

    /// <summary>
    /// Converts a string to kebab-case.
    /// Examples: "ToDo" -> "to-do", "toDo" -> "to-do"
    /// </summary>
    public static string ToKebabCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Kebaberize();
    }

    /// <summary>
    /// Converts a string to snake_case.
    /// Examples: "ToDo" -> "to_do", "toDo" -> "to_do"
    /// </summary>
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Underscore();
    }

    /// <summary>
    /// Pluralizes a word.
    /// Examples: "ToDo" -> "ToDos", "Category" -> "Categories"
    /// </summary>
    public static string Pluralize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Pluralize();
    }

    /// <summary>
    /// Singularizes a word.
    /// Examples: "ToDos" -> "ToDo", "Categories" -> "Category"
    /// </summary>
    public static string Singularize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Singularize();
    }

    /// <summary>
    /// Humanizes a string for display.
    /// Examples: "toDo" -> "To do", "ToDoItem" -> "To do item"
    /// </summary>
    public static string Humanize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Humanize();
    }

    /// <summary>
    /// Converts to title case for display.
    /// Examples: "toDo" -> "To Do", "to_do_item" -> "To Do Item"
    /// </summary>
    public static string ToTitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input.Humanize(LetterCasing.Title);
    }
}
