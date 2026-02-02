namespace DataBuilder.Cli.Configuration;

public class DataBuilderOptions
{
    public const string SectionName = "DataBuilder";

    public CouchbaseDefaults Couchbase { get; set; } = new();
    public EditorOptions Editor { get; set; } = new();
    public AngularOptions Angular { get; set; } = new();
}

public class CouchbaseDefaults
{
    public string Bucket { get; set; } = "general";
    public string Scope { get; set; } = "general";
    public bool UseTypeDiscriminator { get; set; } = false;
}

public class EditorOptions
{
    public string? PreferredEditor { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(30);
}

public class AngularOptions
{
    public int DevServerPort { get; set; } = 4200;
    public bool SkipInstall { get; set; } = true;
    public bool AddMaterial { get; set; } = true;
}
