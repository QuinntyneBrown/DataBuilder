using System.CommandLine;
using DataBuilder.Cli.Commands;
using DataBuilder.Cli.Generators.Angular;
using DataBuilder.Cli.Generators.Api;
using DataBuilder.Cli.Services;
using DataBuilder.Cli.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Create root command
        var rootCommand = new RootCommand("DataBuilder CLI - Scaffold full-stack applications with C# API and Angular frontend");

        // Add solution-create command
        var solutionCreateCommand = new SolutionCreateCommand();
        rootCommand.Subcommands.Add(solutionCreateCommand);

        // Set handler for solution-create
        solutionCreateCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var name = parseResult.GetValue(solutionCreateCommand.NameOption);
            var directory = parseResult.GetValue(solutionCreateCommand.DirectoryOption);
            var jsonFile = parseResult.GetValue(solutionCreateCommand.JsonFileOption);

            var handler = serviceProvider.GetRequiredService<SolutionCreateCommandHandler>();
            return await handler.HandleAsync(name!, directory!, jsonFile, cancellationToken);
        });

        // Invoke
        return await rootCommand.Parse(args).InvokeAsync();
    }

    static void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Utilities
        services.AddSingleton<IProcessRunner, ProcessRunner>();

        // Services
        services.AddSingleton<IJsonEditorService, JsonEditorService>();
        services.AddSingleton<ISchemaParser, SchemaParser>();
        services.AddSingleton<ISolutionGenerator, SolutionGenerator>();

        // Generators
        services.AddSingleton<IApiGenerator, ApiGenerator>();
        services.AddSingleton<IAngularGenerator, AngularGenerator>();

        // Command handlers
        services.AddSingleton<SolutionCreateCommandHandler>();
    }
}
