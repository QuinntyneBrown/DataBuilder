using System.Reflection;
using Microsoft.Extensions.Logging;
using Scriban;

namespace DataBuilder.Cli.Templates;

public interface ITemplateFactory
{
    Template GetTemplate(string name);
    bool HasTemplate(string name);
    IReadOnlyList<string> AvailableTemplates { get; }
}

public class EmbeddedResourceTemplateFactory : ITemplateFactory
{
    private readonly Lazy<Dictionary<string, Template>> _templates;
    private readonly ILogger<EmbeddedResourceTemplateFactory> _logger;

    public EmbeddedResourceTemplateFactory(ILogger<EmbeddedResourceTemplateFactory> logger)
    {
        _logger = logger;
        _templates = new Lazy<Dictionary<string, Template>>(LoadAllTemplates);
    }

    public Template GetTemplate(string name)
    {
        if (!_templates.Value.TryGetValue(name, out var template))
        {
            throw new Exceptions.TemplateNotFoundException(name);
        }
        return template;
    }

    public bool HasTemplate(string name) => _templates.Value.ContainsKey(name);

    public IReadOnlyList<string> AvailableTemplates => _templates.Value.Keys.ToList();

    private Dictionary<string, Template> LoadAllTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var templates = new Dictionary<string, Template>();

        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".sbn")))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);

            var templateName = ExtractTemplateName(resourceName);
            var content = reader.ReadToEnd();
            var template = Template.Parse(content);

            if (template.HasErrors)
            {
                _logger.LogError("Template {Name} has errors: {Errors}",
                    templateName, string.Join(", ", template.Messages));
                continue;
            }

            templates[templateName] = template;
            _logger.LogDebug("Loaded template: {Name}", templateName);
        }

        return templates;
    }

    private static string ExtractTemplateName(string resourceName)
    {
        // DataBuilder.Cli.Templates.Api.Entity.sbn -> Entity
        var parts = resourceName.Split('.');
        return parts[^2]; // Second to last
    }
}
