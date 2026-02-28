namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Represents a form builder section component definition.
/// </summary>
public class FormBuilderSectionDefinition : IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the form section.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the display name of the form section.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the form section.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the icon CSS class for the form section.
    /// </summary>
    public string? IconClass { get; }

    /// <summary>
    /// Gets the type (component) for rendering the form section.
    /// </summary>
    public Type? MarkedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormBuilderSectionDefinition"/> class.
    /// </summary>
    public FormBuilderSectionDefinition(
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
