using CMS.Core;

using Kentico.Builder.Web.Mvc;

namespace Kentico.Xperience.Labs.ComponentRegistry;

/// <summary>
/// Localization adapter for component registry read models.
/// </summary>
public interface IComponentRegistryLocalizationService
{
    /// <summary>
    /// Resolves and localizes a display string.
    /// </summary>
    public string Localize(string value);

    /// <summary>
    /// Resolves a display string that can be a plain resource key (e.g. automation step registrations) or contain localization macros.
    /// </summary>
    public string LocalizeResourceKey(string value);
}

/// <summary>
/// Default implementation using Kentico admin builder localization.
/// </summary>
public class ComponentRegistryLocalizationService(
    IAdminBuildersLocalizationService localizer,
    ILocalizationService localizationService) : IComponentRegistryLocalizationService
{
    /// <inheritdoc/>
    public string Localize(string value) => localizer.LocalizeString(value);

    /// <inheritdoc/>
    public string LocalizeResourceKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (value.Contains("{$", StringComparison.Ordinal))
        {
            return Localize(value);
        }

        return value.Any(char.IsWhiteSpace)
            ? value
            : localizationService.GetString(value);
    }
}
