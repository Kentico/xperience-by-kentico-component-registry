using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Shared constants of the "Builder structure (Labs)" tabs of web pages (<see cref="WebPageBuilderStructurePage"/>)
/// and emails (<see cref="EmailBuilderStructurePage"/>).
/// </summary>
internal static class BuilderStructureTab
{
    public const string SLUG = "builder-structure";

    /// <summary>
    /// Tab name. "(Labs)" tells editors the tab comes from this Labs extension, not the product.
    /// </summary>
    public const string NAME = "Builder structure (Labs)";

    public const string TEMPLATE_NAME = "@kentico/xperience-labs-component-registry-admin/BuilderStructure";
}

/// <summary>
/// Values of <see cref="BuilderStructureClientProperties.Builder"/>.
/// </summary>
public static class BuilderStructureBuilders
{
    public const string PAGE_BUILDER = "pageBuilder";
    public const string EMAIL_BUILDER = "emailBuilder";
}

public class BuilderStructureClientProperties : TemplateClientProperties
{
    /// <summary>
    /// One of <see cref="BuilderStructureBuilders"/>.
    /// </summary>
    public string Builder { get; set; } = BuilderStructureBuilders.PAGE_BUILDER;

    public string LanguageName { get; set; } = string.Empty;

    /// <summary>
    /// <c>null</c> when the web page or email has no variant in <see cref="LanguageName"/>.
    /// </summary>
    public BuilderStructureDto? Structure { get; set; }
}
