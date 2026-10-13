using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Websites.UIPages;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

[assembly: UIPage(
    uiPageType: typeof(PageBuilderComponentViewerPage),
    parentType: typeof(ComponentRegistryApplicationPage),
    slug: "page-builder",
    name: "Page Builder",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 1,
    Icon = Icons.CustomElement)]
[assembly: UIPage(
    uiPageType: typeof(PageBuilderWidgetsPage),
    parentType: typeof(PageBuilderComponentViewerPage),
    slug: "widgets",
    name: "Widgets",
    templateName: PageBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 1)]
[assembly: UIPage(
    uiPageType: typeof(PageBuilderSectionsPage),
    parentType: typeof(PageBuilderComponentViewerPage),
    slug: "sections",
    name: "Sections",
    templateName: PageBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 2)]
[assembly: UIPage(
    uiPageType: typeof(PageBuilderTemplatesPage),
    parentType: typeof(PageBuilderComponentViewerPage),
    slug: "page-templates",
    name: "Page Templates",
    templateName: PageBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 3)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Page Builder section. Redirects to its first child page.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
[UINavigation(true)]
public class PageBuilderComponentViewerPage : SecondaryMenuSectionPage
{
    public const string TEMPLATE_NAME = "@kentico/xperience-labs-component-registry-admin/PageBuilderComponentViewer";
}

/// <summary>
/// Base page for displaying registered Page Builder components of a single kind.
/// Each page sends only its own component definitions to the client.
/// </summary>
/// <typeparam name="TComponent">Component definition DTO sent to the client.</typeparam>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public abstract class PageBuilderComponentListingPage<TComponent>(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IUIPermissionEvaluator permissionEvaluator) : Page<PageBuilderComponentViewerPageClientProperties<TComponent>>
{
    /// <summary>
    /// Component kind sent to the client: <c>widget</c>, <c>section</c> or <c>template</c>.
    /// </summary>
    protected abstract string ComponentKind { get; }

    protected abstract IReadOnlyList<TComponent> GetComponents(PageBuilderRegistryReadModel model);

    protected abstract Task<ComponentUsageDetailDto> GetUsage(string identifier);

    public override async Task<PageBuilderComponentViewerPageClientProperties<TComponent>> ConfigureTemplateProperties(
        PageBuilderComponentViewerPageClientProperties<TComponent> properties)
    {
        var model = await componentRegistryReadService.GetPageBuilderRegistryAsync();

        properties.ComponentKind = ComponentKind;
        properties.Components = GetComponents(model);

        var canViewUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_PAGE_BUILDER_USAGES);
        properties.CanViewUsages = canViewUsages.Succeeded;

        return properties;
    }

    /// <summary>
    /// Retrieves detailed usage information for a Page Builder component of this page's kind.
    /// </summary>
    [PageCommand(CommandName = "GetPageBuilderComponentUsage", Permission = ComponentRegistryPermissions.VIEW_PAGE_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetPageBuilderComponentUsage(ComponentDetailsParams @params)
    {
        if (@params?.ComponentIdentifier is null)
        {
            return ResponseFrom("Component identifier is required");
        }

        var usage = await GetUsage(@params.ComponentIdentifier);

        foreach (var page in usage.Pages)
        {
            foreach (var variant in page.Variants)
            {
                string adminPath = pageLinkGenerator.GetPath<PageBuilderTab>(
                    new PageParameterValues()
                    {
                        { typeof(WebPageLayout), $"{variant.LanguageName}_{page.WebPageItemId}" },
                        { typeof(WebPagesApplication), $"webpages-{page.WebsiteChannelID}" },
                    });

                variant.AdminPath = adminPath.StartsWith('/')
                    ? adminPath[1..]
                    : adminPath;
            }
        }

        return ResponseFrom(usage);
    }
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public class PageBuilderWidgetsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : PageBuilderComponentListingPage<ComponentDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "widget";

    protected override IReadOnlyList<ComponentDto> GetComponents(PageBuilderRegistryReadModel model) => model.Widgets;

    protected override Task<ComponentUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetPageBuilderWidgetUsageAsync(identifier);
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public class PageBuilderSectionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : PageBuilderComponentListingPage<ComponentDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "section";

    protected override IReadOnlyList<ComponentDto> GetComponents(PageBuilderRegistryReadModel model) => model.Sections;

    protected override Task<ComponentUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetPageBuilderSectionUsageAsync(identifier);
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public class PageBuilderTemplatesPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : PageBuilderComponentListingPage<PageTemplateDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "template";

    protected override IReadOnlyList<PageTemplateDto> GetComponents(PageBuilderRegistryReadModel model) => model.PageTemplates;

    protected override Task<ComponentUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetPageBuilderPageTemplateUsageAsync(identifier);
}

/// <summary>
/// Client properties for a Page Builder component listing page.
/// </summary>
/// <typeparam name="TComponent">Component definition DTO.</typeparam>
public class PageBuilderComponentViewerPageClientProperties<TComponent> : TemplateClientProperties
{
    public IReadOnlyList<TComponent> Components { get; set; } = [];
    public string ComponentKind { get; set; } = string.Empty;
    public bool CanViewUsages { get; set; }
}
