namespace TestSolution.Core.Models;

/// <summary>
/// Represents a Product entity.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid ProductId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the inStock.
    /// </summary>
    public bool InStock { get; set; }

    /// <summary>
    /// Gets or sets the releaseDate.
    /// </summary>
    public DateTime ReleaseDate { get; set; }
}
