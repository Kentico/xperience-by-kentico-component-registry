namespace Kentico.Xperience.Labs.ComponentRegistry;

/// <summary>
/// Registered name and icon of a builder component.
/// </summary>
internal sealed record RegisteredBuilderComponent(string Name, string? IconClass);

/// <summary>
/// Case-insensitive lookup of the registered templates, sections, and widgets of one builder (Page Builder or Email Builder).
/// </summary>
internal sealed class BuilderComponentLookup
{
    private readonly Dictionary<string, RegisteredBuilderComponent> templates;
    private readonly Dictionary<string, RegisteredBuilderComponent> sections;
    private readonly Dictionary<string, RegisteredBuilderComponent> widgets;

    private BuilderComponentLookup(
        IEnumerable<(string Identifier, string Name, string? IconClass)> templates,
        IEnumerable<(string Identifier, string Name, string? IconClass)> sections,
        IEnumerable<(string Identifier, string Name, string? IconClass)> widgets)
    {
        this.templates = ToLookup(templates);
        this.sections = ToLookup(sections);
        this.widgets = ToLookup(widgets);
    }

    public static BuilderComponentLookup From(PageBuilderRegistryReadModel registry) => new(
        registry.PageTemplates.Select(t => (t.Identifier, t.Name, t.IconClass)),
        registry.Sections.Select(s => (s.Identifier, s.Name, s.IconClass)),
        registry.Widgets.Select(w => (w.Identifier, w.Name, w.IconClass)));

    public static BuilderComponentLookup From(EmailBuilderRegistryReadModel registry) => new(
        registry.EmailTemplates.Select(t => (t.Identifier, t.Name, t.IconClass)),
        registry.Sections.Select(s => (s.Identifier, s.Name, s.IconClass)),
        registry.Widgets.Select(w => (w.Identifier, w.Name, w.IconClass)));

    public RegisteredBuilderComponent? FindTemplate(string? identifier) => Find(templates, identifier);

    public RegisteredBuilderComponent? FindSection(string? identifier) => Find(sections, identifier);

    public RegisteredBuilderComponent? FindWidget(string? identifier) => Find(widgets, identifier);

    private static RegisteredBuilderComponent? Find(Dictionary<string, RegisteredBuilderComponent> lookup, string? identifier) =>
        identifier is not null && lookup.TryGetValue(identifier, out var component) ? component : null;

    private static Dictionary<string, RegisteredBuilderComponent> ToLookup(IEnumerable<(string Identifier, string Name, string? IconClass)> components) =>
        components
            .DistinctBy(c => c.Identifier, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(c => c.Identifier, c => new RegisteredBuilderComponent(c.Name, c.IconClass), StringComparer.OrdinalIgnoreCase);
}
