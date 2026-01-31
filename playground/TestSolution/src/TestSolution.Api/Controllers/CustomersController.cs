using TestSolution.Core.Models;
using TestSolution.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace TestSolution.Api.Controllers;

/// <summary>
/// API controller for Customers CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerRepository repository,
        ILogger<CustomersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all customers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Customer>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all Customers");
        var items = await _repository.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Gets a paged list of customers.
    /// </summary>
    [HttpGet("page")]
    [ProducesResponseType(typeof(PagedResult<Customer>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<Customer>>> GetPage(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting page {PageIndex} of Customers with size {PageSize}", pageIndex, pageSize);
        var result = await _repository.GetPageAsync(pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Customer>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Customer with ID: {Id}", id);
        var item = await _repository.GetByIdAsync(id, cancellationToken);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Customer>> Create([FromBody] Customer entity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new Customer");
        var item = await _repository.CreateAsync(entity, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.CustomerId }, item);
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Customer>> Update(Guid id, [FromBody] Customer entity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating Customer with ID: {Id}", id);

        if (id != entity.CustomerId)
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
    /// Deletes a customer.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting Customer with ID: {Id}", id);
        var deleted = await _repository.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
