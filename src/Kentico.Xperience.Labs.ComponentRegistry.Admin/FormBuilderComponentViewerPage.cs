using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

[assembly: UIPage(
    uiPageType: typeof(FormBuilderComponentViewerPage),
    parentType: typeof(ComponentRegistryApplicationPage),
    slug: "form-builder",
    name: "Form Builder",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 3,
    Icon = Icons.CustomElement)]
[assembly: UIPage(
    uiPageType: typeof(FormBuilderComponentsPage),
    parentType: typeof(FormBuilderComponentViewerPage),
    slug: "components",
    name: "Components",
    templateName: FormBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 1)]
[assembly: UIPage(
    uiPageType: typeof(FormBuilderSectionsPage),
    parentType: typeof(FormBuilderComponentViewerPage),
    slug: "sections",
    name: "Sections",
    templateName: FormBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 2)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Form Builder section. Redirects to its first child page.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
[UINavigation(true)]
public class FormBuilderComponentViewerPage : SecondaryMenuSectionPage
{
    public const string TEMPLATE_NAME = "@kentico/xperience-labs-component-registry-admin/FormBuilderComponentViewer";
}

/// <summary>
/// Base page for displaying registered Form Builder components of a single kind.
/// Each page sends only its own component definitions to the client.
/// </summary>
/// <typeparam name="TComponent">Component definition DTO sent to the client.</typeparam>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
public abstract class FormBuilderComponentListingPage<TComponent>(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IUIPermissionEvaluator permissionEvaluator) : Page<FormBuilderComponentViewerPageClientProperties<TComponent>>
{
    /// <summary>
    /// Component kind sent to the client: <c>component</c> or <c>section</c>.
    /// </summary>
    protected abstract string ComponentKind { get; }

    protected abstract IReadOnlyList<TComponent> GetComponents(FormBuilderRegistryReadModel model);

    protected abstract Task<FormComponentUsageDetailDto> GetUsage(string identifier);

    public override async Task<FormBuilderComponentViewerPageClientProperties<TComponent>> ConfigureTemplateProperties(
        FormBuilderComponentViewerPageClientProperties<TComponent> properties)
    {
        var model = await componentRegistryReadService.GetFormBuilderRegistryAsync();

        properties.ComponentKind = ComponentKind;
        properties.Components = GetComponents(model);

        var canViewUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_FORM_BUILDER_USAGES);
        properties.CanViewUsages = canViewUsages.Succeeded;

        return properties;
    }

    /// <summary>
    /// Retrieves detailed usage information for a Form Builder component of this page's kind.
    /// </summary>
    [PageCommand(CommandName = "GetFormBuilderComponentUsage", Permission = ComponentRegistryPermissions.VIEW_FORM_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetFormBuilderComponentUsage(ComponentDetailsParams @params)
    {
        if (@params?.ComponentIdentifier is null)
        {
            return ResponseFrom("Component identifier is required");
        }

        var usage = await GetUsage(@params.ComponentIdentifier);

        foreach (var form in usage.FormBuilderForms)
        {
            string adminPath = pageLinkGenerator.GetPath<FormBuilderTab>(
                new PageParameterValues()
                {
                    { typeof(FormEditSection), form.FormID.ToString() },
                });

            form.AdminPath = adminPath.StartsWith('/')
                ? adminPath[1..]
                : adminPath;
        }

        return ResponseFrom(usage);
    }
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
public class FormBuilderComponentsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : FormBuilderComponentListingPage<FormComponentDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "component";

    protected override IReadOnlyList<FormComponentDto> GetComponents(FormBuilderRegistryReadModel model) => model.FormComponents;

    protected override Task<FormComponentUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetFormBuilderComponentUsageAsync(identifier);
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
public class FormBuilderSectionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : FormBuilderComponentListingPage<FormSectionDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override string ComponentKind => "section";

    protected override IReadOnlyList<FormSectionDto> GetComponents(FormBuilderRegistryReadModel model) => model.FormSections;

    protected override Task<FormComponentUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetFormBuilderSectionUsageAsync(identifier);
}

/// <summary>
/// Client properties for a Form Builder component listing page.
/// </summary>
/// <typeparam name="TComponent">Component definition DTO.</typeparam>
public class FormBuilderComponentViewerPageClientProperties<TComponent> : TemplateClientProperties
{
    public IReadOnlyList<TComponent> Components { get; set; } = [];
    public string ComponentKind { get; set; } = string.Empty;
    public bool CanViewUsages { get; set; }
}
