namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Represents a section component definition.
/// </summary>
public class PageBuilderSectionDefinition : IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the section.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the display name of the section.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the section.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the icon CSS class for the section.
    /// </summary>
    public string? IconClass { get; }

    /// <summary>
    /// Gets the type (view component or controller) for rendering the section.
    /// </summary>
    public Type? MarkedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBuilderSectionDefinition"/> class.
    /// </summary>
    public PageBuilderSectionDefinition(
        string identifier,
        string name,
        Type? markedType = null,
        string? description = null,
        string? iconClass = null)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MarkedType = markedType;
        Description = description;
        IconClass = iconClass;
    }
}
