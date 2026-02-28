namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Service to store and retrieve component definitions.
/// </summary>
/// <typeparam name="TDefinition">The type of definition stored.</typeparam>
public interface IComponentDefinitionStore<TDefinition>
{
    /// <summary>
    /// Gets a component definition by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the component definition.</param>
    /// <returns>The component definition, or default if not found.</returns>
    public TDefinition? Get(string identifier);

    /// <summary>
    /// Adds a component definition to the store.
    /// </summary>
    /// <param name="registeredDefinition">The definition to add.</param>
    public void Add(TDefinition registeredDefinition);

    /// <summary>
    /// Gets all component definitions in the store.
    /// </summary>
    /// <returns>All registered component definitions.</returns>
    public IEnumerable<TDefinition> GetAll();
}


/// <summary>
/// Component definition store that stores and retrieves component definitions by identifier.
/// </summary>
/// <typeparam name="TDefinition">The type of definition stored.</typeparam>
internal class ComponentDefinitionStore<TDefinition> : IComponentDefinitionStore<TDefinition>
    where TDefinition : class, IComponentDefinition
{
    private readonly DefinitionRegister<string, TDefinition> registeredDefinitions = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Gets a component definition by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the component definition.</param>
    /// <returns>The component definition, or null if not found.</returns>
    public TDefinition? Get(string identifier)
    {
        if (registeredDefinitions.TryGetValue(identifier, out var definition))
        {
            return definition;
        }
        return default;
    }

    /// <summary>
    /// Adds a component definition to the store.
    /// </summary>
    /// <param name="registeredDefinition">The definition to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registeredDefinition"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a definition with the same identifier already exists.</exception>
    public void Add(TDefinition registeredDefinition)
    {
        if (registeredDefinition == null)
        {
            throw new ArgumentNullException(nameof(registeredDefinition));
        }

        if (!registeredDefinitions.TryAddUnsafe(registeredDefinition.Identifier, registeredDefinition))
        {
            throw new ArgumentException($"Definition with key '{registeredDefinition.Identifier}' cannot be registered because another definition with such key is already present.");
        }
    }

    /// <summary>
    /// Gets all component definitions in the store.
    /// </summary>
    /// <returns>All registered component definitions.</returns>
    public IEnumerable<TDefinition> GetAll() => registeredDefinitions.GetAll();
}
