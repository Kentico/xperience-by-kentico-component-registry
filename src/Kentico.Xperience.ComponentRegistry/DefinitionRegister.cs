namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// A thread-safe register of definitions.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the register.</typeparam>
/// <typeparam name="TValue">The type of the definition in the register.</typeparam>
/// <remarks>
/// The register is optimized for read operations while modifications to the register are less efficient.
/// </remarks>
internal class DefinitionRegister<TKey, TValue> where TKey : notnull
{
    private volatile Dictionary<TKey, TValue> register;
    private readonly IEqualityComparer<TKey> comparer;


    /// <summary>
    /// Initializes a new instance of the <see cref="DefinitionRegister{TKey, TValue}"/> class that uses the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="comparer">Comparer to be used for keys.</param>
    public DefinitionRegister(IEqualityComparer<TKey> comparer)
    {
        this.comparer = comparer;
#pragma warning disable IDE0028
        register = new(comparer);
#pragma warning restore IDE0028
    }


    /// <summary>
    /// Tries to add a new key and value to the register, if given <paramref name="key"/> is not already present.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The element to be added.</param>
    /// <returns>Returns true if register did not contain <paramref name="key"/> and the new element was added. Otherwise returns false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    public bool TryAdd(TKey key, TValue value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        Dictionary<TKey, TValue> initialRegister;
        Dictionary<TKey, TValue> newRegister;

        do
        {
            initialRegister = register;
            if (initialRegister.ContainsKey(key))
            {
                return false;
            }

            newRegister = new Dictionary<TKey, TValue>(initialRegister, comparer)
            {
                { key, value }
            };
        } while (Interlocked.CompareExchange(ref register, newRegister, initialRegister) != initialRegister);

        return true;
    }


    /// <summary>
    /// Tries to add a new key and value to the register, if given <paramref name="key"/> is not already present. This method is not thread-safe.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The element to be added.</param>
    /// <returns>Returns true if register did not contain <paramref name="key"/> and the new element was added. Otherwise returns false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    /// <remarks>
    /// This member is not thread-safe while providing optimal performance of the modification operation.
    /// </remarks>
    public bool TryAddUnsafe(TKey key, TValue value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (register.ContainsKey(key))
        {
            return false;
        }

        register.Add(key, value);

        return true;
    }


    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">Value associated with the key, if found, or the default value for the <typeparamref name="TValue"/> type.</param>
    /// <returns>Returns true if <paramref name="key"/> is found. Otherwise returns false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return register.TryGetValue(key, out value);
    }


    /// <summary>
    /// Gets all values registered within the register.
    /// </summary>
    /// <returns>Returns enumeration of all registered values.</returns>
    public IEnumerable<TValue> GetAll() => register.Values;
}
