using DataBuilder.Cli.Exceptions;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli;

public static class ExceptionHandler
{
    public static int Handle(Exception ex, ILogger logger)
    {
        switch (ex)
        {
            case SchemaParseException spe:
                logger.LogError("Invalid JSON schema: {Message}", spe.Message);
                Console.Error.WriteLine($"Error: Invalid JSON schema - {spe.Message}");
                return 2;

            case TemplateNotFoundException tnfe:
                logger.LogError(tnfe, "Missing template: {Template}", tnfe.TemplateName);
                Console.Error.WriteLine($"Error: Required template missing - {tnfe.TemplateName}");
                return 3;

            case InvalidSolutionStructureException isse:
                logger.LogError("Invalid solution: {Path} - {Message}", isse.SolutionPath, isse.Message);
                Console.Error.WriteLine($"Error: {isse.Message}");
                return 4;

            case ProcessExecutionException pee:
                logger.LogError(pee, "Process failed: {Command}", pee.Command);
                Console.Error.WriteLine($"Error: {pee.Command} failed");
                if (!string.IsNullOrEmpty(pee.StandardError))
                    Console.Error.WriteLine(pee.StandardError);
                return 5;

            case OperationCanceledException:
                logger.LogWarning("Operation cancelled");
                Console.Error.WriteLine("Operation cancelled.");
                return 130; // Standard SIGINT exit code

            default:
                logger.LogError(ex, "Unexpected error");
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                return 1;
        }
    }
}
