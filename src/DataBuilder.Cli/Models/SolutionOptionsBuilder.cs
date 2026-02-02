namespace DataBuilder.Cli.Models;

/// <summary>
/// Builder for creating SolutionOptions with a fluent API.
/// </summary>
public class SolutionOptionsBuilder
{
    private string _name = string.Empty;
    private string _directory = string.Empty;
    private readonly List<EntityDefinition> _entities = new();
    private string _bucket = "general";
    private string _scope = "general";
    private int _apiPort = 5000;
    private int _angularPort = 4200;

    public SolutionOptionsBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SolutionOptionsBuilder WithDirectory(string directory)
    {
        _directory = directory;
        return this;
    }

    public SolutionOptionsBuilder AddEntity(EntityDefinition entity)
    {
        _entities.Add(entity);
        return this;
    }

    public SolutionOptionsBuilder AddEntities(IEnumerable<EntityDefinition> entities)
    {
        _entities.AddRange(entities);
        return this;
    }

    public SolutionOptionsBuilder WithCouchbase(string bucket, string scope)
    {
        _bucket = bucket;
        _scope = scope;
        return this;
    }

    public SolutionOptionsBuilder WithApiPort(int port)
    {
        _apiPort = port;
        return this;
    }

    public SolutionOptionsBuilder WithAngularPort(int port)
    {
        _angularPort = port;
        return this;
    }

    public SolutionOptions Build()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Solution name is required");

        return new SolutionOptions
        {
            Name = _name,
            Directory = _directory,
            Entities = _entities,
            DefaultBucket = _bucket,
            DefaultScope = _scope,
            ApiPort = _apiPort,
            AngularPort = _angularPort
        };
    }
}
