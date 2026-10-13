using CMS.Websites.Internal;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Websites;
using Kentico.Xperience.Admin.Websites.UIPages;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

[assembly: UIPage(
    uiPageType: typeof(WebPageBuilderStructurePage),
    parentType: typeof(WebPageLayout),
    slug: BuilderStructureTab.SLUG,
    name: BuilderStructureTab.NAME,
    templateName: BuilderStructureTab.TEMPLATE_NAME,
    order: WebPageBuilderStructurePage.ORDER,
    Icon = Icons.CustomElement)]

[assembly: PageExtender(typeof(WebPageBuilderStructureNavigationExtender))]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// "Builder structure (Labs)" tab of a web page: the Page Builder configuration of the latest version of the edited language variant
/// as nested boxes (page template, editable areas, sections, zones, widgets, widget variants).
/// Requires <see cref="ComponentRegistryPermissions.VIEW_PAGE_BUILDER"/> in the Component Registry application, on top of the
/// website channel permissions that already guard <see cref="WebPageLayout"/>.
/// </summary>
internal sealed class WebPageBuilderStructurePage(
    IBuilderStructureService builderStructureService,
    IComponentRegistryPermissionEvaluator permissionEvaluator) : Page<BuilderStructureClientProperties>
{
    /// <summary>
    /// Directly after "page-builder" (<see cref="PageBuilderTab"/>, 300) and before "usage" (350).
    /// </summary>
    public const int ORDER = 325;

    private readonly IBuilderStructureService builderStructureService = builderStructureService;
    private readonly IComponentRegistryPermissionEvaluator permissionEvaluator = permissionEvaluator;

    [PageParameter(typeof(WebPageUrlIdentifierPageModelBinder), typeof(WebPageLayout))]
    public WebPageUrlIdentifier WebPageIdentifier { get; set; } = new(string.Empty, 0);

    // The website channel root has no web page item (the tab is hidden there too).
    public override async Task<PageValidationResult> ValidatePage()
    {
        await permissionEvaluator.EnsureGranted(ComponentRegistryPermissions.VIEW_PAGE_BUILDER);

        return new()
        {
            IsValid = WebPageIdentifier.WebPageItemID != WebPageConstants.ROOT_NODE_ID,
            ErrorMessageKey = "base.forms.error.objectnotinitialized",
        };
    }

    public override async Task<BuilderStructureClientProperties> ConfigureTemplateProperties(BuilderStructureClientProperties properties)
    {
        await permissionEvaluator.EnsureGranted(ComponentRegistryPermissions.VIEW_PAGE_BUILDER);

        properties.Builder = BuilderStructureBuilders.PAGE_BUILDER;
        properties.LanguageName = WebPageIdentifier.LanguageName;
        properties.Structure = await builderStructureService.GetWebPageStructureAsync(WebPageIdentifier.WebPageItemID, WebPageIdentifier.LanguageName);

        return properties;
    }
}

/// <summary>
/// Hides the "Builder structure (Labs)" tab on the website channel root and for users without
/// <see cref="ComponentRegistryPermissions.VIEW_PAGE_BUILDER"/>. The page itself checks the permission too.
/// </summary>
internal sealed class WebPageBuilderStructureNavigationExtender(IComponentRegistryPermissionEvaluator permissionEvaluator) : PageExtender<WebPageLayout>
{
    private readonly IComponentRegistryPermissionEvaluator permissionEvaluator = permissionEvaluator;

    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        properties = await base.ConfigureTemplateProperties(properties);

        if (Page.WebPageIdentifier?.WebPageItemID == WebPageConstants.ROOT_NODE_ID
            || !await permissionEvaluator.IsGranted(ComponentRegistryPermissions.VIEW_PAGE_BUILDER))
        {
            properties.Navigation.Items = [.. properties.Navigation.Items
                .Where(item => !string.Equals(item.Path, BuilderStructureTab.SLUG, StringComparison.OrdinalIgnoreCase))];
        }

        return properties;
    }
}
