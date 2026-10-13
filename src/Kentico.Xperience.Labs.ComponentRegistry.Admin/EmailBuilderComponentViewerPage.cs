using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

using InternalEmailBuilderTab = Kentico.Xperience.Admin.DigitalMarketing.UIPages.Internal.EmailBuilderTab;

[assembly: UIPage(
    uiPageType: typeof(EmailBuilderComponentViewerPage),
    parentType: typeof(ComponentRegistryApplicationPage),
    slug: "email-builder",
    name: "Email Builder",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 2,
    Icon = Icons.CustomElement)]
[assembly: UIPage(
    uiPageType: typeof(EmailBuilderWidgetsPage),
    parentType: typeof(EmailBuilderComponentViewerPage),
    slug: "widgets",
    name: "Widgets",
    templateName: "@kentico/xperience-labs-component-registry-admin/EmailBuilderComponentViewer",
    order: 1)]
[assembly: UIPage(
    uiPageType: typeof(EmailBuilderSectionsPage),
    parentType: typeof(EmailBuilderComponentViewerPage),
    slug: "sections",
    name: "Sections",
    templateName: "@kentico/xperience-labs-component-registry-admin/EmailBuilderComponentViewer",
    order: 2)]
[assembly: UIPage(
    uiPageType: typeof(EmailBuilderTemplatesPage),
    parentType: typeof(EmailBuilderComponentViewerPage),
    slug: "email-templates",
    name: "Email Templates",
    templateName: "@kentico/xperience-labs-component-registry-admin/EmailBuilderComponentViewer",
    order: 3)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Email Builder section. Redirects to its first child page.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
[UINavigation(true)]
public class EmailBuilderComponentViewerPage : SecondaryMenuSectionPage;

/// <summary>
/// Base page for displaying registered Email Builder components of a single type.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public abstract class EmailBuilderComponentListingPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator) : Page<EmailBuilderComponentViewerPageClientProperties>
{
    protected abstract string ComponentView { get; }

    public override async Task<EmailBuilderComponentViewerPageClientProperties> ConfigureTemplateProperties(
        EmailBuilderComponentViewerPageClientProperties properties)
    {
        properties.ComponentView = ComponentView;

        var model = await componentRegistryReadService.GetEmailBuilderRegistryAsync();

        properties.Widgets = model.Widgets;
        properties.Sections = model.Sections;
        properties.EmailTemplates = model.EmailTemplates;

        // Evaluate permissions and propagate to client
        var canViewEmailBuilderUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_EMAIL_BUILDER_USAGES);
        properties.CanViewEmailBuilderUsages = canViewEmailBuilderUsages.Succeeded;

        return properties;
    }

    /// <summary>
    /// Retrieves detailed usage information for an email builder widget component.
    /// </summary>
    [PageCommand(CommandName = "GetEmailBuilderWidgetUsage", Permission = ComponentRegistryPermissions.VIEW_EMAIL_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetEmailBuilderWidgetUsage(ComponentDetailsParams @params)
    {
        var usage = await componentUsageService.GetEmailBuilderWidgetUsageAsync(@params.ComponentIdentifier);
        AddAdminPaths(usage);
        return ResponseFrom(usage);
    }

    /// <summary>
    /// Retrieves detailed usage information for an email builder template component.
    /// </summary>
    [PageCommand(CommandName = "GetEmailBuilderTemplateUsage", Permission = ComponentRegistryPermissions.VIEW_EMAIL_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetEmailBuilderTemplateUsage(ComponentDetailsParams @params)
    {
        var usage = await componentUsageService.GetEmailBuilderTemplateUsageAsync(@params.ComponentIdentifier);
        AddAdminPaths(usage);
        return ResponseFrom(usage);
    }

    private void AddAdminPaths(EmailConfigurationUsageDetailDto usage)
    {
        foreach (var configuration in usage.EmailConfigurations)
        {
            foreach (var variant in configuration.Variants)
            {
                string adminPath = pageLinkGenerator.GetPath<InternalEmailBuilderTab>(
                    new PageParameterValues()
                    {
                        { typeof(EmailEditLayout), configuration.EmailConfigurationId.ToString() },
                        { typeof(EmailChannelContentLanguage), variant.LanguageName },
                        { typeof(EmailChannelApplication), $"emails-{configuration.EmailChannelID}" },
                    });

                variant.AdminPath = adminPath.StartsWith('/')
                    ? adminPath[1..]
                    : adminPath;
            }
        }
    }
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public class EmailBuilderWidgetsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : EmailBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "widgets";
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public class EmailBuilderSectionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : EmailBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "sections";
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public class EmailBuilderTemplatesPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : EmailBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "templates";
}

/// <summary>
/// Client properties for the email builder component viewer page.
/// </summary>
public class EmailBuilderComponentViewerPageClientProperties : TemplateClientProperties
{
    public IEnumerable<EmailComponentDto> Widgets { get; set; } = [];
    public IEnumerable<EmailComponentDto> Sections { get; set; } = [];
    public IEnumerable<EmailTemplateDto> EmailTemplates { get; set; } = [];
    public bool CanViewEmailBuilderUsages { get; set; }
    public string ComponentView { get; set; } = string.Empty;
}
