using TestSolution.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Couchbase.Extensions.DependencyInjection;

namespace TestSolution.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    public static IServiceCollection AddTestSolutionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Couchbase
        services.AddCouchbase(configuration.GetSection("Couchbase"));

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
