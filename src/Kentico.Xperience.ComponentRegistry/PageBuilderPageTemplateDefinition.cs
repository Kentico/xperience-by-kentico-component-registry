namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Represents a page template component definition.
/// </summary>
public class PageBuilderPageTemplateDefinition : IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the page template.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the display name of the page template.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the page template.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the icon CSS class for the page template.
    /// </summary>
    public string? IconClass { get; }

    /// <summary>
    /// Gets the type (view component or controller) for rendering the page template.
    /// </summary>
    public Type? MarkedType { get; }

    /// <summary>
    /// Gets the content type names that can use this page template.
    /// </summary>
    public string[] ContentTypeNames { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBuilderPageTemplateDefinition"/> class.
    /// </summary>
    public PageBuilderPageTemplateDefinition(
        string identifier,
        string name,
        Type? markedType = null,
        string? description = null,
        string? iconClass = null,
        string[]? contentTypeNames = null)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MarkedType = markedType;
        Description = description;
        IconClass = iconClass;
        ContentTypeNames = contentTypeNames ?? Array.Empty<string>();
    }
}
