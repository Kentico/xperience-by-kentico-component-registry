namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Represents an email builder widget component definition.
/// </summary>
public class EmailBuilderWidgetDefinition : IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the email widget.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the display name of the email widget.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the email widget.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the icon CSS class for the email widget.
    /// </summary>
    public string? IconClass { get; }

    /// <summary>
    /// Gets the type (component) for rendering the email widget.
    /// </summary>
    public Type? MarkedType { get; }

    /// <summary>
    /// Gets the properties type for the email widget.
    /// </summary>
    public Type? PropertiesType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailBuilderWidgetDefinition"/> class.
    /// </summary>
    public EmailBuilderWidgetDefinition(
        string identifier,
        string name,
        Type? markedType = null,
        string? description = null,
        string? iconClass = null,
        Type? propertiesType = null)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MarkedType = markedType;
        Description = description;
        IconClass = iconClass;
        PropertiesType = propertiesType;
    }
}
