namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Represents a form builder form component definition.
/// </summary>
public class FormBuilderComponentDefinition : IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the form component.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the display name of the form component.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the form component.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the icon CSS class for the form component.
    /// </summary>
    public string? IconClass { get; }

    /// <summary>
    /// Gets the type (component) for rendering the form component.
    /// </summary>
    public Type? MarkedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormBuilderComponentDefinition"/> class.
    /// </summary>
    public FormBuilderComponentDefinition(
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
