namespace Kentico.Xperience.Labs.ComponentRegistry;

/// <summary>
/// Represents an Admin UI form component definition registered with
/// <see cref="Admin.Base.Forms.RegisterFormComponentAttribute"/>.
/// Not related to Form Builder form components.
/// </summary>
public class AdminFormComponentDefinition : IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the form component, stored in <c>field/settings/controlname</c> of class form definitions.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the name of the form component.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Always null; the Admin UI form component registration has no description.
    /// </summary>
    public string? Description => null;

    /// <summary>
    /// Always null; the Admin UI form component registration has no icon.
    /// </summary>
    public string? IconClass => null;

    /// <summary>
    /// Gets the type implementing the form component.
    /// </summary>
    public Type? MarkedType { get; }

    /// <summary>
    /// Gets whether the component can be assigned to a field in the field editor.
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminFormComponentDefinition"/> class.
    /// </summary>
    public AdminFormComponentDefinition(
        string identifier,
        string name,
        Type? markedType = null,
        bool isAvailable = true)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MarkedType = markedType;
        IsAvailable = isAvailable;
    }
}
