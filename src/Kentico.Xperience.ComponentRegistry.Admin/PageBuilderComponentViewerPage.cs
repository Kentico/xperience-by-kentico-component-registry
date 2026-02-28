using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Websites.UIPages;

using Kentico.Xperience.ComponentRegistry.Admin;

[assembly: UIPage(
    uiPageType: typeof(PageBuilderComponentViewerPage),
    parentType: typeof(ComponentRegistryApplicationPage),
    slug: "page-builder",
    name: "Page Builder",
    templateName: "@kentico/xperience-integrations-component-registry-web-admin/PageBuilderComponentViewer",
    order: 1,
    Icon = Icons.CustomElement)]

namespace Kentico.Xperience.ComponentRegistry.Admin;

/// <summary>
/// Page for displaying all registered component definitions.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER)]
public class PageBuilderComponentViewerPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator) : Page<PageBuilderComponentViewerPageClientProperties>
{
    public override async Task<PageBuilderComponentViewerPageClientProperties> ConfigureTemplateProperties(
        PageBuilderComponentViewerPageClientProperties properties)
    {
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

/// <summary>
/// Client properties for the component viewer page.
/// </summary>
public class PageBuilderComponentViewerPageClientProperties : TemplateClientProperties
{
    public IEnumerable<ComponentDto> Widgets { get; set; } = [];
    public IEnumerable<ComponentDto> Sections { get; set; } = [];
    public IEnumerable<PageTemplateDto> PageTemplates { get; set; } = [];
    public bool CanViewPageBuilderUsages { get; set; }
}
