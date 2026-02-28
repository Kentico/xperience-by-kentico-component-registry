namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Base interface for component definitions.
/// </summary>
public interface IComponentDefinition
{
    /// <summary>
    /// Gets the unique identifier of the component.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Gets the display name of the component.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the component.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the icon CSS class for the component.
    /// </summary>
    public string? IconClass { get; }
}
