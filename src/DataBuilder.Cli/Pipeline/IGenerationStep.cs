using DataBuilder.Cli.Common;
using DataBuilder.Cli.Models;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli.Pipeline;

public interface IGenerationStep
{
    string Name { get; }
    int Order { get; }
    Task<Result> ExecuteAsync(GenerationContext context, CancellationToken ct);
}

public class GenerationContext
{
    public SolutionOptions Options { get; init; } = null!;
    public EntityDefinition? Entity { get; set; }
    public Dictionary<string, object> State { get; } = new();
}

public class GenerationPipeline
{
    private readonly IEnumerable<IGenerationStep> _steps;
    private readonly ILogger<GenerationPipeline> _logger;

    public GenerationPipeline(IEnumerable<IGenerationStep> steps, ILogger<GenerationPipeline> logger)
    {
        _steps = steps.OrderBy(s => s.Order);
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(GenerationContext context, CancellationToken ct)
    {
        foreach (var step in _steps)
        {
            _logger.LogInformation("Executing step: {StepName}", step.Name);
            var result = await step.ExecuteAsync(context, ct);
            if (!result.IsSuccess)
            {
                _logger.LogError("Step failed: {StepName} - {Error}", step.Name, result.Error);
                return result;
            }
        }
        return Result.Success();
    }
}
