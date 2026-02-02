namespace DataBuilder.Cli.Configuration;

public static class Defaults
{
    public const string CouchbaseBucket = "general";
    public const string CouchbaseScope = "general";
    public const int AngularPort = 4200;
    public const int ApiPort = 5001;
}

public static class ProjectPaths
{
    public const string Src = "src";
    public const string Models = "Models";
    public const string Data = "Data";
    public const string Controllers = "Controllers";
    public const string Features = "features";
    public const string Services = "services";
}

public static class FileExtensions
{
    public const string CSharp = ".cs";
    public const string TypeScript = ".ts";
    public const string Html = ".html";
    public const string Scss = ".scss";
    public const string Json = ".json";
}

public static class RegexPatterns
{
    public const string RepositoryRegistration = @"services\.AddScoped<I\w+Repository, \w+Repository>\(\);";
    public const string RouteEntry = @"path:\s*'([^']+)'";
    public const string NavigationItem = @"<mat-nav-list>";
}
