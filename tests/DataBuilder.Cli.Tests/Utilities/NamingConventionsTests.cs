using DataBuilder.Cli.Utilities;
using FluentAssertions;

namespace DataBuilder.Cli.Tests.Utilities;

public class NamingConventionsTests
{
    #region ToPascalCase Tests

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("helloWorld", "HelloWorld")]
    [InlineData("hello_world", "HelloWorld")]
    [InlineData("HelloWorld", "HelloWorld")]
    [InlineData("", "")]
    [InlineData("a", "A")]
    public void ToPascalCase_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.ToPascalCase(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void ToPascalCase_WithNull_ShouldReturnNull()
    {
        var result = NamingConventions.ToPascalCase(null!);
        result.Should().BeNull();
    }

    #endregion

    #region ToCamelCase Tests

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("HelloWorld", "helloWorld")]
    [InlineData("hello_world", "helloWorld")]
    [InlineData("helloWorld", "helloWorld")]
    [InlineData("", "")]
    [InlineData("A", "a")]
    public void ToCamelCase_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.ToCamelCase(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void ToCamelCase_WithNull_ShouldReturnEmpty()
    {
        var result = NamingConventions.ToCamelCase(null!);
        result.Should().BeNullOrEmpty();
    }

    #endregion

    #region ToKebabCase Tests

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("helloWorld", "hello-world")]
    [InlineData("", "")]
    [InlineData("A", "a")]
    public void ToKebabCase_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.ToKebabCase(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void ToKebabCase_WithNull_ShouldReturnEmpty()
    {
        var result = NamingConventions.ToKebabCase(null!);
        result.Should().BeNullOrEmpty();
    }

    #endregion

    #region ToSnakeCase Tests

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("helloWorld", "hello_world")]
    [InlineData("", "")]
    [InlineData("A", "a")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.ToSnakeCase(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void ToSnakeCase_WithNull_ShouldReturnEmpty()
    {
        var result = NamingConventions.ToSnakeCase(null!);
        result.Should().BeNullOrEmpty();
    }

    #endregion

    #region Pluralize Tests

    [Theory]
    [InlineData("Product", "Products")]
    [InlineData("Category", "Categories")]
    [InlineData("Person", "People")]
    [InlineData("Child", "Children")]
    [InlineData("Mouse", "Mice")]
    [InlineData("Box", "Boxes")]
    [InlineData("Bus", "Buses")]
    [InlineData("Quiz", "Quizzes")]
    [InlineData("", "")]
    public void Pluralize_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.Pluralize(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void Pluralize_WithNull_ShouldReturnEmpty()
    {
        var result = NamingConventions.Pluralize(null!);
        result.Should().BeNullOrEmpty();
    }

    #endregion

    #region Singularize Tests

    [Theory]
    [InlineData("Products", "Product")]
    [InlineData("Categories", "Category")]
    [InlineData("People", "Person")]
    [InlineData("Children", "Child")]
    [InlineData("Mice", "Mouse")]
    [InlineData("Boxes", "Box")]
    [InlineData("Buses", "Bus")]
    [InlineData("", "")]
    public void Singularize_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.Singularize(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void Singularize_WithNull_ShouldReturnEmpty()
    {
        var result = NamingConventions.Singularize(null!);
        result.Should().BeNullOrEmpty();
    }

    #endregion

    #region ToTitleCase Tests

    [Theory]
    [InlineData("helloWorld", "Hello World")]
    [InlineData("HelloWorld", "Hello World")]
    [InlineData("hello_world", "Hello World")]
    [InlineData("hello-world", "Hello World")]
    [InlineData("", "")]
    public void ToTitleCase_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.ToTitleCase(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void ToTitleCase_WithNull_ShouldReturnEmpty()
    {
        var result = NamingConventions.ToTitleCase(null!);
        result.Should().BeNullOrEmpty();
    }

    #endregion

    #region Humanize Tests

    [Theory]
    [InlineData("hello_world", "hello world")]
    [InlineData("", "")]
    public void Humanize_ShouldConvertCorrectly(string input, string expected)
    {
        var result = NamingConventions.Humanize(input);
        result.Should().Be(expected);
    }

    [Fact]
    public void Humanize_WithNull_ShouldReturnEmpty()
    {
        var result = NamingConventions.Humanize(null!);
        result.Should().BeNullOrEmpty();
    }

    #endregion
}
