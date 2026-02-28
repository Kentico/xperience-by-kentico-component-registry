namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Represents a widget component definition.
/// </summary>
public class PageBuilderWidgetDefinition : IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the widget.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the display name of the widget.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the widget.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the icon CSS class for the widget.
    /// </summary>
    public string? IconClass { get; }

    /// <summary>
    /// Gets the type (view component or controller) for rendering the widget.
    /// </summary>
    public Type? MarkedType { get; }

    /// <summary>
    /// Gets whether the widget output can be cached.
    /// </summary>
    public bool AllowCache { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBuilderWidgetDefinition"/> class.
    /// </summary>
    public PageBuilderWidgetDefinition(
        string identifier,
        string name,
        Type? markedType = null,
        string? description = null,
        string? iconClass = null,
        bool allowCache = false)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MarkedType = markedType;
        Description = description;
        IconClass = iconClass;
        AllowCache = allowCache;
    }
}
