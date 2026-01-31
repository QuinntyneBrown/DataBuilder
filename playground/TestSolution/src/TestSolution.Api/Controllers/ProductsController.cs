using TestSolution.Core.Models;
using TestSolution.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace TestSolution.Api.Controllers;

/// <summary>
/// API controller for Products CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository repository,
        ILogger<ProductsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all Products");
        var items = await _repository.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Gets a paged list of products.
    /// </summary>
    [HttpGet("page")]
    [ProducesResponseType(typeof(PagedResult<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Product>>> GetPage(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting page {PageIndex} of Products with size {PageSize}", pageIndex, pageSize);
        var result = await _repository.GetPageAsync(pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Product with ID: {Id}", id);
        var item = await _repository.GetByIdAsync(id, cancellationToken);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> Create([FromBody] Product entity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new Product");
        var item = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.ProductId }, item);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> Update(Guid id, [FromBody] Product entity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Product with ID: {Id}", id);

        if (id != entity.ProductId)
        {
            return BadRequest("ID mismatch");
        }

        var item = await _repository.UpdateAsync(entity, cancellationToken);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Product with ID: {Id}", id);
        var deleted = await _repository.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
