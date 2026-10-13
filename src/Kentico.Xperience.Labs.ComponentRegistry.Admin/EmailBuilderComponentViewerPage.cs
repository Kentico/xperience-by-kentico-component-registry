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
    templateName: EmailBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 1)]
[assembly: UIPage(
    uiPageType: typeof(EmailBuilderSectionsPage),
    parentType: typeof(EmailBuilderComponentViewerPage),
    slug: "sections",
    name: "Sections",
    templateName: EmailBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 2)]
[assembly: UIPage(
    uiPageType: typeof(EmailBuilderTemplatesPage),
    parentType: typeof(EmailBuilderComponentViewerPage),
    slug: "email-templates",
    name: "Email Templates",
    templateName: EmailBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 3)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Email Builder section. Redirects to its first child page.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
[UINavigation(true)]
public class EmailBuilderComponentViewerPage : SecondaryMenuSectionPage
{
    public const string TEMPLATE_NAME = "@kentico/xperience-labs-component-registry-admin/EmailBuilderComponentViewer";
}

/// <summary>
/// Base page for displaying registered Email Builder components of a single kind.
/// Each page sends only its own component definitions to the client.
/// </summary>
/// <typeparam name="TComponent">Component definition DTO sent to the client.</typeparam>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public abstract class EmailBuilderComponentListingPage<TComponent>(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IUIPermissionEvaluator permissionEvaluator) : Page<EmailBuilderComponentViewerPageClientProperties<TComponent>>
{
    /// <summary>
    /// Component kind sent to the client: <c>widget</c>, <c>section</c> or <c>template</c>.
    /// </summary>
    protected abstract string ComponentKind { get; }

    protected abstract IReadOnlyList<TComponent> GetComponents(EmailBuilderRegistryReadModel model);

    protected abstract Task<EmailConfigurationUsageDetailDto> GetUsage(string identifier);

    public override async Task<EmailBuilderComponentViewerPageClientProperties<TComponent>> ConfigureTemplateProperties(
        EmailBuilderComponentViewerPageClientProperties<TComponent> properties)
    {
        var model = await componentRegistryReadService.GetEmailBuilderRegistryAsync();

        properties.ComponentKind = ComponentKind;
        properties.Components = GetComponents(model);

        var canViewUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_EMAIL_BUILDER_USAGES);
        properties.CanViewUsages = canViewUsages.Succeeded;

        return properties;
    }

    /// <summary>
    /// Retrieves detailed usage information for an Email Builder component of this page's kind.
    /// </summary>
    [PageCommand(CommandName = "GetEmailBuilderComponentUsage", Permission = ComponentRegistryPermissions.VIEW_EMAIL_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetEmailBuilderComponentUsage(ComponentDetailsParams @params)
    {
        if (@params?.ComponentIdentifier is null)
        {
            return ResponseFrom("Component identifier is required");
        }

        var usage = await GetUsage(@params.ComponentIdentifier);

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

        return ResponseFrom(usage);
    }
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public class EmailBuilderWidgetsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : EmailBuilderComponentListingPage<EmailComponentDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "widget";

    protected override IReadOnlyList<EmailComponentDto> GetComponents(EmailBuilderRegistryReadModel model) => model.Widgets;

    protected override Task<EmailConfigurationUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetEmailBuilderWidgetUsageAsync(identifier);
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public class EmailBuilderSectionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : EmailBuilderComponentListingPage<EmailComponentDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "section";

    protected override IReadOnlyList<EmailComponentDto> GetComponents(EmailBuilderRegistryReadModel model) => model.Sections;

    protected override Task<EmailConfigurationUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetEmailBuilderSectionUsageAsync(identifier);
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)]
public class EmailBuilderTemplatesPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : EmailBuilderComponentListingPage<EmailTemplateDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "template";

    protected override IReadOnlyList<EmailTemplateDto> GetComponents(EmailBuilderRegistryReadModel model) => model.EmailTemplates;

    protected override Task<EmailConfigurationUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetEmailBuilderTemplateUsageAsync(identifier);
}

/// <summary>
/// Client properties for an Email Builder component listing page.
/// </summary>
/// <typeparam name="TComponent">Component definition DTO.</typeparam>
public class EmailBuilderComponentViewerPageClientProperties<TComponent> : TemplateClientProperties
{
    public IReadOnlyList<TComponent> Components { get; set; } = [];
    public string ComponentKind { get; set; } = string.Empty;
    public bool CanViewUsages { get; set; }
}
