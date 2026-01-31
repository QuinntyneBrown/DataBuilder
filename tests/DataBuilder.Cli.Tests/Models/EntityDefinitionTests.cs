using DataBuilder.Cli.Models;
using FluentAssertions;

namespace DataBuilder.Cli.Tests.Models;

public class EntityDefinitionTests
{
    #region Computed Name Properties Tests

    [Fact]
    public void NameCamelCase_ShouldConvertCorrectly()
    {
        var entity = new EntityDefinition { Name = "ProductCategory" };
        entity.NameCamelCase.Should().Be("productCategory");
    }

    [Fact]
    public void NameKebabCase_ShouldConvertCorrectly()
    {
        var entity = new EntityDefinition { Name = "ProductCategory" };
        entity.NameKebabCase.Should().Be("product-category");
    }

    [Fact]
    public void NamePlural_ShouldPluralize()
    {
        var entity = new EntityDefinition { Name = "Product" };
        entity.NamePlural.Should().Be("Products");
    }

    [Fact]
    public void NamePlural_WithCategory_ShouldPluralize()
    {
        var entity = new EntityDefinition { Name = "Category" };
        entity.NamePlural.Should().Be("Categories");
    }

    [Fact]
    public void NamePluralCamelCase_ShouldConvertCorrectly()
    {
        var entity = new EntityDefinition { Name = "Product" };
        entity.NamePluralCamelCase.Should().Be("products");
    }

    [Fact]
    public void NamePluralKebabCase_ShouldConvertCorrectly()
    {
        var entity = new EntityDefinition { Name = "ProductCategory" };
        entity.NamePluralKebabCase.Should().Be("product-categories");
    }

    [Fact]
    public void DisplayName_ShouldConvertToTitleCase()
    {
        var entity = new EntityDefinition { Name = "ProductCategory" };
        entity.DisplayName.Should().Be("Product Category");
    }

    [Fact]
    public void DisplayNamePlural_ShouldConvertToTitleCase()
    {
        var entity = new EntityDefinition { Name = "ProductCategory" };
        entity.DisplayNamePlural.Should().Be("Product Categories");
    }

    #endregion

    #region IdProperty Tests

    [Fact]
    public void IdProperty_WhenExists_ReturnsProperty()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", IsId = true },
                new() { Name = "Name", IsId = false }
            }
        };

        entity.IdProperty.Should().NotBeNull();
        entity.IdProperty!.Name.Should().Be("Id");
    }

    [Fact]
    public void IdProperty_WhenNotExists_ReturnsNull()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Name", IsId = false }
            }
        };

        entity.IdProperty.Should().BeNull();
    }

    [Fact]
    public void IdPropertyName_WhenIdExists_ReturnsIdName()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "ProductId", IsId = true }
            }
        };

        entity.IdPropertyName.Should().Be("ProductId");
    }

    [Fact]
    public void IdPropertyName_WhenNoIdExists_ReturnsDefaultId()
    {
        var entity = new EntityDefinition { Name = "Product" };
        entity.IdPropertyName.Should().Be("Id");
    }

    [Fact]
    public void IdPropertyNameCamelCase_WhenIdExists_ReturnsCamelCase()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "ProductId", NameCamelCase = "productId", IsId = true }
            }
        };

        entity.IdPropertyNameCamelCase.Should().Be("productId");
    }

    [Fact]
    public void IdPropertyNameCamelCase_WhenNoIdExists_ReturnsDefaultId()
    {
        var entity = new EntityDefinition { Name = "Product" };
        entity.IdPropertyNameCamelCase.Should().Be("id");
    }

    #endregion

    #region NonIdProperties Tests

    [Fact]
    public void NonIdProperties_ShouldExcludeIdProperty()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id", IsId = true },
                new() { Name = "Name", IsId = false },
                new() { Name = "Price", IsId = false }
            }
        };

        entity.NonIdProperties.Should().HaveCount(2);
        entity.NonIdProperties.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Name", "Price" });
    }

    [Fact]
    public void NonIdProperties_WhenNoId_ReturnsAllProperties()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Name", IsId = false },
                new() { Name = "Price", IsId = false }
            }
        };

        entity.NonIdProperties.Should().HaveCount(2);
    }

    #endregion

    #region ListDisplayProperties Tests

    [Fact]
    public void ListDisplayProperties_ShouldOnlyIncludeAllowedNames()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id" },
                new() { Name = "Name" },
                new() { Name = "Description" },
                new() { Name = "Price" },
                new() { Name = "Category" },
                new() { Name = "Version" },
                new() { Name = "VersionNumber" }
            }
        };

        entity.ListDisplayProperties.Select(p => p.Name)
            .Should().BeEquivalentTo(new[] { "Id", "Name", "Description", "Version", "VersionNumber" });
    }

    [Fact]
    public void ListDisplayProperties_ShouldBeLimitedToFive()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition>
            {
                new() { Name = "Id" },
                new() { Name = "Version" },
                new() { Name = "VersionNumber" },
                new() { Name = "Name" },
                new() { Name = "Description" },
                new() { Name = "ExtraAllowed" } // Not in allowed list
            }
        };

        entity.ListDisplayProperties.Should().HaveCount(5);
    }

    #endregion

    #region HasNameProperty and HasDescriptionProperty Tests

    [Fact]
    public void HasNameProperty_WhenExists_ReturnsTrue()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition> { new() { Name = "Name" } }
        };

        entity.HasNameProperty.Should().BeTrue();
    }

    [Fact]
    public void HasNameProperty_WhenNotExists_ReturnsFalse()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition> { new() { Name = "Title" } }
        };

        entity.HasNameProperty.Should().BeFalse();
    }

    [Fact]
    public void HasDescriptionProperty_WhenExists_ReturnsTrue()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition> { new() { Name = "Description" } }
        };

        entity.HasDescriptionProperty.Should().BeTrue();
    }

    [Fact]
    public void HasDescriptionProperty_WhenNotExists_ReturnsFalse()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            Properties = new List<PropertyDefinition> { new() { Name = "Summary" } }
        };

        entity.HasDescriptionProperty.Should().BeFalse();
    }

    #endregion

    #region Icon Tests

    [Theory]
    [InlineData("User", "person")]
    [InlineData("Account", "account_circle")]
    [InlineData("Category", "category")]
    [InlineData("Product", "inventory_2")]
    [InlineData("Order", "shopping_cart")]
    [InlineData("Idea", "lightbulb")]
    [InlineData("Task", "task_alt")]
    [InlineData("Project", "folder")]
    [InlineData("Document", "description")]
    [InlineData("Setting", "settings")]
    [InlineData("Role", "admin_panel_settings")]
    [InlineData("Permission", "security")]
    [InlineData("Notification", "notifications")]
    [InlineData("Message", "message")]
    [InlineData("Comment", "comment")]
    [InlineData("Report", "assessment")]
    [InlineData("Dashboard", "dashboard")]
    [InlineData("Log", "history")]
    [InlineData("File", "attach_file")]
    [InlineData("Image", "image")]
    [InlineData("Video", "videocam")]
    [InlineData("Event", "event")]
    [InlineData("Calendar", "calendar_today")]
    [InlineData("Contact", "contacts")]
    [InlineData("Customer", "people")]
    [InlineData("Employee", "badge")]
    [InlineData("Team", "groups")]
    [InlineData("UnknownEntity", "list")]
    public void Icon_ShouldReturnCorrectIcon(string entityName, string expectedIcon)
    {
        var entity = new EntityDefinition { Name = entityName };
        entity.Icon.Should().Be(expectedIcon);
    }

    #endregion

    #region Couchbase Settings Tests

    [Fact]
    public void UseTypeDiscriminator_DefaultIsFalse()
    {
        var entity = new EntityDefinition { Name = "Product" };
        entity.UseTypeDiscriminator.Should().BeFalse();
    }

    [Fact]
    public void Bucket_DefaultIsGeneral()
    {
        var entity = new EntityDefinition { Name = "Product" };
        entity.Bucket.Should().Be("general");
    }

    [Fact]
    public void Scope_DefaultIsGeneral()
    {
        var entity = new EntityDefinition { Name = "Product" };
        entity.Scope.Should().Be("general");
    }

    [Fact]
    public void Collection_WhenNoDiscriminator_ReturnsEntityName()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            UseTypeDiscriminator = false
        };

        entity.Collection.Should().Be("product");
    }

    [Fact]
    public void Collection_WhenUsingDiscriminator_ReturnsGeneral()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            UseTypeDiscriminator = true
        };

        entity.Collection.Should().Be("general");
    }

    [Fact]
    public void Collection_WhenOverrideSet_ReturnsOverride()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            CollectionOverride = "custom_collection"
        };

        entity.Collection.Should().Be("custom_collection");
    }

    [Fact]
    public void Collection_WhenOverrideSetWithDiscriminator_ReturnsOverride()
    {
        var entity = new EntityDefinition
        {
            Name = "Product",
            UseTypeDiscriminator = true,
            CollectionOverride = "custom_collection"
        };

        entity.Collection.Should().Be("custom_collection");
    }

    #endregion
}
