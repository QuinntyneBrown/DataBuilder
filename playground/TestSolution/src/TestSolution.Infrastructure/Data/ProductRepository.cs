using TestSolution.Core.Models;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.KeyValue;
using Microsoft.Extensions.Logging;
using Gateway.Core;

namespace TestSolution.Infrastructure.Data;

/// <summary>
/// Repository interface for Product data access.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetPageAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(Product entity, CancellationToken cancellationToken = default);
    Task<Product?> UpdateAsync(Product entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for Product using Couchbase and Gateway.Core.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly IBucketProvider _bucketProvider;
    private readonly ILogger<ProductRepository> _logger;
    private const string BucketName = "";
    private const string DocumentKeyPrefix = "product::";

    public ProductRepository(
        IBucketProvider bucketProvider,
        ILogger<ProductRepository> logger)
    {
        _bucketProvider = bucketProvider;
        _logger = logger;
    }

    private async Task<ICouchbaseCollection> GetCollectionAsync()
    {
        var bucket = await _bucketProvider.GetBucketAsync(BucketName);
        return await bucket.DefaultCollectionAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();
        var key = $"{DocumentKeyPrefix}{id}";

        try
        {
            var result = await collection.GetAsync(key);
            return result.ContentAs<Product>();
        }
        catch (Couchbase.Core.Exceptions.KeyValue.DocumentNotFoundException)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bucket = await _bucketProvider.GetBucketAsync(BucketName);
        var cluster = bucket.Cluster;

        var query = $"SELECT META().id, d.* FROM `{BucketName}` AS d WHERE META().id LIKE '{DocumentKeyPrefix}%'";
        var result = await cluster.QueryAsync<Product>(query);

        var items = new List<Product>();
        await foreach (var item in result)
        {
            items.Add(item);
        }

        return items;
    }

    public async Task<PagedResult<Product>> GetPageAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
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
        var dataResult = await cluster.QueryAsync<Product>(dataQuery);

        var items = new List<Product>();
        await foreach (var item in dataResult)
        {
            items.Add(item);
        }

        return new PagedResult<Product>
        {
            Data = items,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = (int)totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<Product> CreateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();

        if (entity.ProductId == Guid.Empty)
        {
            entity.ProductId = Guid.NewGuid();
        }

        var key = $"{DocumentKeyPrefix}{entity.ProductId}";
        await collection.InsertAsync(key, entity);

        _logger.LogInformation("Created Product with ID: {Id}", entity.ProductId);
        return entity;
    }

    public async Task<Product?> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var collection = await GetCollectionAsync();
        var key = $"{DocumentKeyPrefix}{entity.ProductId}";

        try
        {
            await collection.ReplaceAsync(key, entity);
            _logger.LogInformation("Updated Product with ID: {Id}", entity.ProductId);
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
            _logger.LogInformation("Deleted Product with ID: {Id}", id);
            return true;
        }
        catch (Couchbase.Core.Exceptions.KeyValue.DocumentNotFoundException)
        {
            return false;
        }
    }
}
