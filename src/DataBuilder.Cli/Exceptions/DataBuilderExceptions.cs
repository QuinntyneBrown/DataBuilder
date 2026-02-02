namespace DataBuilder.Cli.Exceptions;

/// <summary>
/// Base exception for all DataBuilder operations.
/// </summary>
public abstract class DataBuilderException : Exception
{
    public string? Context { get; init; }

    protected DataBuilderException(string message) : base(message) { }
    protected DataBuilderException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when JSON schema parsing fails.
/// </summary>
public class SchemaParseException : DataBuilderException
{
    public string? JsonContent { get; init; }

    public SchemaParseException(string message) : base(message) { }
    public SchemaParseException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a required template is not found.
/// </summary>
public class TemplateNotFoundException : DataBuilderException
{
    public string TemplateName { get; }

    public TemplateNotFoundException(string templateName)
        : base($"Template not found: {templateName}")
    {
        TemplateName = templateName;
    }
}

/// <summary>
/// Thrown when template rendering fails.
/// </summary>
public class TemplateRenderException : DataBuilderException
{
    public string TemplateName { get; }

    public TemplateRenderException(string templateName, Exception inner)
        : base($"Failed to render template: {templateName}", inner)
    {
        TemplateName = templateName;
    }
}

/// <summary>
/// Thrown when solution structure validation fails.
/// </summary>
public class InvalidSolutionStructureException : DataBuilderException
{
    public string SolutionPath { get; }

    public InvalidSolutionStructureException(string solutionPath, string message)
        : base(message)
    {
        SolutionPath = solutionPath;
    }
}

/// <summary>
/// Thrown when external process execution fails.
/// </summary>
public class ProcessExecutionException : DataBuilderException
{
    public string Command { get; }
    public int ExitCode { get; }
    public string? StandardError { get; }

    public ProcessExecutionException(string command, int exitCode, string? stderr)
        : base($"Command '{command}' failed with exit code {exitCode}")
    {
        Command = command;
        ExitCode = exitCode;
        StandardError = stderr;
    }
}
