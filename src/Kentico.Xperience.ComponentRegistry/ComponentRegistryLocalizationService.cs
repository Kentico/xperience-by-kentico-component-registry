using Kentico.Builder.Web.Mvc;

namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Localization adapter for component registry read models.
/// </summary>
public interface IComponentRegistryLocalizationService
{
    /// <summary>
    /// Resolves and localizes a display string.
    /// </summary>
    public string Localize(string value);
}

/// <summary>
/// Default implementation using Kentico admin builder localization.
/// </summary>
public class ComponentRegistryLocalizationService(IAdminBuildersLocalizationService localizer) : IComponentRegistryLocalizationService
{
    /// <inheritdoc/>
    public string Localize(string value) => localizer.LocalizeString(value);
}
