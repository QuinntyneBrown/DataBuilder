using DataBuilder.Cli.Models;
using FluentAssertions;

namespace DataBuilder.Cli.Tests.Models;

public class SolutionOptionsTests
{
    [Fact]
    public void NamePascalCase_WithKebabCase_ConvertsToPascalCase()
    {
        var options = new SolutionOptions { Name = "my-app" };
        options.NamePascalCase.Should().Be("MyApp");
    }

    [Fact]
    public void NamePascalCase_WithUnderscores_ConvertsToPascalCase()
    {
        var options = new SolutionOptions { Name = "my_app" };
        options.NamePascalCase.Should().Be("MyApp");
    }

    [Fact]
    public void NamePascalCase_AlreadyPascalCase_StaysSame()
    {
        var options = new SolutionOptions { Name = "MyApp" };
        options.NamePascalCase.Should().Be("MyApp");
    }

    [Fact]
    public void NameKebabCase_ConvertsToPascalCase()
    {
        var options = new SolutionOptions { Name = "MyApp" };
        options.NameKebabCase.Should().Be("my-app");
    }

    [Fact]
    public void SolutionDirectory_CombinesDirectoryAndName()
    {
        var options = new SolutionOptions
        {
            Name = "my-app",
            Directory = "/projects"
        };

        options.SolutionDirectory.Should().EndWith("MyApp");
        options.SolutionDirectory.Should().Contain("projects");
    }

    [Fact]
    public void SrcDirectory_IsUnderSolutionDirectory()
    {
        var options = new SolutionOptions
        {
            Name = "my-app",
            Directory = "/projects"
        };

        options.SrcDirectory.Should().EndWith("src");
        options.SrcDirectory.Should().Contain("MyApp");
    }

    [Fact]
    public void CoreProjectName_HasCorrectFormat()
    {
        var options = new SolutionOptions { Name = "my-app" };
        options.CoreProjectName.Should().Be("MyApp.Core");
    }

    [Fact]
    public void InfrastructureProjectName_HasCorrectFormat()
    {
        var options = new SolutionOptions { Name = "my-app" };
        options.InfrastructureProjectName.Should().Be("MyApp.Infrastructure");
    }

    [Fact]
    public void ApiProjectName_HasCorrectFormat()
    {
        var options = new SolutionOptions { Name = "my-app" };
        options.ApiProjectName.Should().Be("MyApp.Api");
    }

    [Fact]
    public void UiProjectName_HasCorrectFormat()
    {
        var options = new SolutionOptions { Name = "my-app" };
        options.UiProjectName.Should().Be("MyApp.Ui");
    }

    [Fact]
    public void CoreProjectDirectory_HasCorrectPath()
    {
        var options = new SolutionOptions
        {
            Name = "my-app",
            Directory = "/projects"
        };

        options.CoreProjectDirectory.Should().EndWith("MyApp.Core");
        options.CoreProjectDirectory.Should().Contain("src");
    }

    [Fact]
    public void InfrastructureProjectDirectory_HasCorrectPath()
    {
        var options = new SolutionOptions
        {
            Name = "my-app",
            Directory = "/projects"
        };

        options.InfrastructureProjectDirectory.Should().EndWith("MyApp.Infrastructure");
        options.InfrastructureProjectDirectory.Should().Contain("src");
    }

    [Fact]
    public void ApiProjectDirectory_HasCorrectPath()
    {
        var options = new SolutionOptions
        {
            Name = "my-app",
            Directory = "/projects"
        };

        options.ApiProjectDirectory.Should().EndWith("MyApp.Api");
        options.ApiProjectDirectory.Should().Contain("src");
    }

    [Fact]
    public void UiProjectDirectory_HasCorrectPath()
    {
        var options = new SolutionOptions
        {
            Name = "my-app",
            Directory = "/projects"
        };

        options.UiProjectDirectory.Should().EndWith("MyApp.Ui");
        options.UiProjectDirectory.Should().Contain("src");
    }

    [Fact]
    public void CouchbaseBucket_ReturnsDefaultBucket()
    {
        var options = new SolutionOptions
        {
            Name = "my-app",
            DefaultBucket = "mybucket"
        };

        options.CouchbaseBucket.Should().Be("mybucket");
    }

    [Fact]
    public void DefaultBucket_DefaultValue()
    {
        var options = new SolutionOptions { Name = "my-app" };
        options.DefaultBucket.Should().Be("general");
    }

    [Fact]
    public void DefaultScope_DefaultValue()
    {
        var options = new SolutionOptions { Name = "my-app" };
        options.DefaultScope.Should().Be("general");
    }

    [Fact]
    public void ApiPort_DefaultValue()
    {
        var options = new SolutionOptions();
        options.ApiPort.Should().Be(5000);
    }

    [Fact]
    public void AngularPort_DefaultValue()
    {
        var options = new SolutionOptions();
        options.AngularPort.Should().Be(4200);
    }

    [Fact]
    public void Entities_DefaultsToEmptyList()
    {
        var options = new SolutionOptions();
        options.Entities.Should().NotBeNull();
        options.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Entities_CanBeSet()
    {
        var entities = new List<EntityDefinition>
        {
            new() { Name = "Product" },
            new() { Name = "Category" }
        };

        var options = new SolutionOptions { Entities = entities };

        options.Entities.Should().HaveCount(2);
        options.Entities.Select(e => e.Name).Should().BeEquivalentTo(new[] { "Product", "Category" });
    }
}
