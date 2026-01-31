namespace TestSolution.Core.Models;

/// <summary>
/// Represents a Customer entity.
/// </summary>
public class Customer
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid CustomerId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the age.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Gets or sets the isActive.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the createdDate.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
