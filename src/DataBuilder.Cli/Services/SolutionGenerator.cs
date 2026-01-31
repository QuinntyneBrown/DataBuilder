using DataBuilder.Cli.Generators.Angular;
using DataBuilder.Cli.Generators.Api;
using DataBuilder.Cli.Models;
using Microsoft.Extensions.Logging;

namespace DataBuilder.Cli.Services;

/// <summary>
/// Orchestrates the generation of a complete solution.
/// </summary>
public class SolutionGenerator : ISolutionGenerator
{
    private readonly IApiGenerator _apiGenerator;
    private readonly IAngularGenerator _angularGenerator;
    private readonly ILogger<SolutionGenerator> _logger;

    public SolutionGenerator(
        IApiGenerator apiGenerator,
        IAngularGenerator angularGenerator,
        ILogger<SolutionGenerator> logger)
    {
        _apiGenerator = apiGenerator;
        _angularGenerator = angularGenerator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task GenerateAsync(SolutionOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating solution: {SolutionName}", options.Name);

        // Create solution directory and src folder
        Directory.CreateDirectory(options.SolutionDirectory);
        Directory.CreateDirectory(options.SrcDirectory);
        _logger.LogInformation("Created solution directory: {Directory}", options.SolutionDirectory);

        // Generate solution file
        await GenerateSolutionFileAsync(options, cancellationToken);

        // Generate API projects (Core, Infrastructure, Api)
        _logger.LogInformation("Generating API projects...");
        await _apiGenerator.GenerateAsync(options, cancellationToken);

        // Generate Angular project in src folder
        _logger.LogInformation("Generating Angular project...");
        await _angularGenerator.GenerateAsync(options, cancellationToken);

        _logger.LogInformation("Solution generation complete: {SolutionDirectory}", options.SolutionDirectory);
    }

    private async Task GenerateSolutionFileAsync(SolutionOptions options, CancellationToken cancellationToken)
    {
        var solutionPath = Path.Combine(options.SolutionDirectory, $"{options.NameKebabCase}.sln");

        var coreProjectGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var infraProjectGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var apiProjectGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();
        var srcFolderGuid = Guid.NewGuid().ToString("D").ToUpperInvariant();

        var solutionContent = $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}"") = ""src"", ""src"", ""{{{srcFolderGuid}}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{options.CoreProjectName}"", ""src\{options.CoreProjectName}\{options.CoreProjectName}.csproj"", ""{{{coreProjectGuid}}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{options.InfrastructureProjectName}"", ""src\{options.InfrastructureProjectName}\{options.InfrastructureProjectName}.csproj"", ""{{{infraProjectGuid}}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{options.ApiProjectName}"", ""src\{options.ApiProjectName}\{options.ApiProjectName}.csproj"", ""{{{apiProjectGuid}}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{{coreProjectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{coreProjectGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{coreProjectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{coreProjectGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{{infraProjectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{infraProjectGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{infraProjectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{infraProjectGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU
		{{{apiProjectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{apiProjectGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{apiProjectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{apiProjectGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{{{coreProjectGuid}}} = {{{srcFolderGuid}}}
		{{{infraProjectGuid}}} = {{{srcFolderGuid}}}
		{{{apiProjectGuid}}} = {{{srcFolderGuid}}}
	EndGlobalSection
EndGlobal
";

        await File.WriteAllTextAsync(solutionPath, solutionContent.TrimStart(), cancellationToken);
        _logger.LogInformation("Created solution file: {SolutionPath}", solutionPath);
    }
}
