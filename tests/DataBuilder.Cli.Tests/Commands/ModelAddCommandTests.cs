using DataBuilder.Cli.Commands;
using FluentAssertions;

namespace DataBuilder.Cli.Tests.Commands;

public class ModelAddCommandTests
{
    [Fact]
    public void Constructor_SetsUpCommandCorrectly()
    {
        var command = new ModelAddCommand();

        command.Name.Should().Be("model-add");
        command.Description.Should().Contain("CRUD operations");
    }

    [Fact]
    public void JsonFileOption_IsOptional()
    {
        var command = new ModelAddCommand();

        command.JsonFileOption.Required.Should().BeFalse();
        command.JsonFileOption.Aliases.Should().Contain("-j");
    }

    [Fact]
    public void UseTypeDiscriminatorOption_IsOptional()
    {
        var command = new ModelAddCommand();

        command.UseTypeDiscriminatorOption.Required.Should().BeFalse();
    }

    [Fact]
    public void BucketOption_HasAlias()
    {
        var command = new ModelAddCommand();

        command.BucketOption.Aliases.Should().Contain("-b");
    }

    [Fact]
    public void ScopeOption_HasAlias()
    {
        var command = new ModelAddCommand();

        command.ScopeOption.Aliases.Should().Contain("-s");
    }

    [Fact]
    public void CollectionOption_HasAlias()
    {
        var command = new ModelAddCommand();

        command.CollectionOption.Aliases.Should().Contain("-c");
    }

    [Fact]
    public void Command_HasAllOptions()
    {
        var command = new ModelAddCommand();

        command.Options.Should().HaveCount(5);
    }
}
