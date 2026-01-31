using TestSolution.Core.Models;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Microsoft.Extensions.Logging;
using Gateway.Core;

namespace TestSolution.Infrastructure.Data;

/// <summary>
/// Repository interface for Customer data access.
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Customer>> GetPageAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Customer> CreateAsync(Customer entity, CancellationToken cancellationToken = default);
    Task<Customer?> UpdateAsync(Customer entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for Customer using Couchbase and Gateway.Core.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly IBucketProvider _bucketProvider;
    private readonly ILogger<CustomerRepository> _logger;
    private const string BucketName = "TestSolution";
    private const string DocumentKeyPrefix = "customer::";

    public CustomerRepository(
        IBucketProvider bucketProvider,
        ILogger<CustomerRepository> logger)
    {
        _bucketProvider = bucketProvider;
        _logger = logger;
    }

    private async Task<ICouchbaseCollection> GetCollectionAsync()
    {
        var bucket = await _bucketProvider.GetBucketAsync(BucketName);
        return await bucket.DefaultCollectionAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();
        var key = $"{DocumentKeyPrefix}{id}";

        try
        {
            var result = await collection.GetAsync(key);
            return result.ContentAs<Customer>();
        }
        catch (Couchbase.Core.Exceptions.KeyValue.DocumentNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bucket = await _bucketProvider.GetBucketAsync(BucketName);
        var cluster = bucket.Cluster;

        var query = $"SELECT META().id, d.* FROM `{BucketName}` AS d WHERE META().id LIKE '{DocumentKeyPrefix}%'";
        var result = await cluster.QueryAsync<Customer>(query);

        var items = new List<Customer>();
        await foreach (var item in result)
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<PagedResult<Customer>> GetPageAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var bucket = await _bucketProvider.GetBucketAsync(BucketName);
        var cluster = bucket.Cluster;

        // Get total count
        var countQuery = $"SELECT COUNT(*) as count FROM `{BucketName}` WHERE META().id LIKE '{DocumentKeyPrefix}%'";
        var countResult = await cluster.QueryAsync<dynamic>(countQuery);
        var countRow = await countResult.Rows.FirstOrDefaultAsync(cancellationToken);
        var totalCount = countRow?.count ?? 0;

        // Get paged data
        var offset = pageIndex * pageSize;
        var dataQuery = $"SELECT META().id, d.* FROM `{BucketName}` AS d WHERE META().id LIKE '{DocumentKeyPrefix}%' LIMIT {pageSize} OFFSET {offset}";
        var dataResult = await cluster.QueryAsync<Customer>(dataQuery);

        var items = new List<Customer>();
        await foreach (var item in dataResult)
        {
            items.Add(item);
        }

        return new PagedResult<Customer>
        {
            Data = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = (int)totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<Customer> CreateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();

        if (entity.CustomerId == Guid.Empty)
        {
            entity.CustomerId = Guid.NewGuid();
        }

        var key = $"{DocumentKeyPrefix}{entity.CustomerId}";
        await collection.InsertAsync(key, entity);

        _logger.LogInformation("Created Customer with ID: {Id}", entity.CustomerId);
        return entity;
    }

    public async Task<Customer?> UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();
        var key = $"{DocumentKeyPrefix}{entity.CustomerId}";

        try
        {
            await collection.ReplaceAsync(key, entity);
            _logger.LogInformation("Updated Customer with ID: {Id}", entity.CustomerId);
            return entity;
        }
        catch (Couchbase.Core.Exceptions.KeyValue.DocumentNotFoundException)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();
        var key = $"{DocumentKeyPrefix}{id}";

        try
        {
            await collection.RemoveAsync(key);
            _logger.LogInformation("Deleted Customer with ID: {Id}", id);
            return true;
        }
        catch (Couchbase.Core.Exceptions.KeyValue.DocumentNotFoundException)
        {
            return false;
        }
    }
}
