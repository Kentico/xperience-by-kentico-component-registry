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
    templateName: "@kentico/xperience-labs-component-registry-admin/PageBuilderComponentViewer",
    order: 1)]
[assembly: UIPage(
    uiPageType: typeof(PageBuilderSectionsPage),
    parentType: typeof(PageBuilderComponentViewerPage),
    slug: "sections",
    name: "Sections",
    templateName: "@kentico/xperience-labs-component-registry-admin/PageBuilderComponentViewer",
    order: 2)]
[assembly: UIPage(
    uiPageType: typeof(PageBuilderTemplatesPage),
    parentType: typeof(PageBuilderComponentViewerPage),
    slug: "page-templates",
    name: "Page Templates",
    templateName: "@kentico/xperience-labs-component-registry-admin/PageBuilderComponentViewer",
    order: 3)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Page Builder section. Redirects to its first child page.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
[UINavigation(true)]
public class PageBuilderComponentViewerPage : SecondaryMenuSectionPage;

/// <summary>
/// Base page for displaying registered Page Builder components of a single type.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public abstract class PageBuilderComponentListingPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator) : Page<PageBuilderComponentViewerPageClientProperties>
{
    protected abstract string ComponentView { get; }

    public override async Task<PageBuilderComponentViewerPageClientProperties> ConfigureTemplateProperties(
        PageBuilderComponentViewerPageClientProperties properties)
    {
        properties.ComponentView = ComponentView;

        var model = await componentRegistryReadService.GetPageBuilderRegistryAsync();

        properties.Widgets = model.Widgets;
        properties.Sections = model.Sections;
        properties.PageTemplates = model.PageTemplates;

        // Evaluate permissions and propagate to client
        var canViewPageBuilderUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_PAGE_BUILDER_USAGES);
        properties.CanViewPageBuilderUsages = canViewPageBuilderUsages.Succeeded;

        return properties;
    }

    /// <summary>
    /// Retrieves detailed usage information for a page builder page template component.
    /// </summary>
    [PageCommand(CommandName = "GetPageBuilderPageTemplateUsage", Permission = ComponentRegistryPermissions.VIEW_PAGE_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetPageBuilderPageTemplateUsage(ComponentDetailsParams @params)
    {
        var usage = await componentUsageService.GetPageBuilderPageTemplateUsageAsync(@params.ComponentIdentifier);
        AddAdminPaths(usage);
        return ResponseFrom(usage);
    }

    /// <summary>
    /// Retrieves detailed usage information for a page builder widget component.
    /// </summary>
    [PageCommand(CommandName = "GetPageBuilderWidgetUsage", Permission = ComponentRegistryPermissions.VIEW_PAGE_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetPageBuilderWidgetUsage(ComponentDetailsParams @params)
    {
        var usage = await componentUsageService.GetPageBuilderWidgetUsageAsync(@params.ComponentIdentifier);
        AddAdminPaths(usage);
        return ResponseFrom(usage);
    }

    private void AddAdminPaths(ComponentUsageDetailDto usage)
    {
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
    }
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public class PageBuilderWidgetsPage(
   IPageLinkGenerator pageLinkGenerator,
   IComponentRegistryReadService componentRegistryReadService,
   IComponentUsageService componentUsageService,
   IUIPermissionEvaluator permissionEvaluator)
   : PageBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "widgets";
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public class PageBuilderSectionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : PageBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "sections";
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public class PageBuilderTemplatesPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : PageBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "templates";
}

/// <summary>
/// Client properties for the component viewer page.
/// </summary>
public class PageBuilderComponentViewerPageClientProperties : TemplateClientProperties
{
    public IEnumerable<ComponentDto> Widgets { get; set; } = [];
    public IEnumerable<ComponentDto> Sections { get; set; } = [];
    public IEnumerable<PageTemplateDto> PageTemplates { get; set; } = [];
    public bool CanViewPageBuilderUsages { get; set; }
    public string ComponentView { get; set; } = string.Empty;
}
