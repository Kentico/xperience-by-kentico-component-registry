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
    templateName: "@kentico/xperience-labs-component-registry-admin/FormBuilderComponentViewer",
    order: 1)]
[assembly: UIPage(
    uiPageType: typeof(FormBuilderSectionsPage),
    parentType: typeof(FormBuilderComponentViewerPage),
    slug: "sections",
    name: "Sections",
    templateName: "@kentico/xperience-labs-component-registry-admin/FormBuilderComponentViewer",
    order: 2)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Form Builder section. Redirects to its first child page.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
[UINavigation(true)]
public class FormBuilderComponentViewerPage : SecondaryMenuSectionPage;

/// <summary>
/// Base page for displaying registered Form Builder components of a single type.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
public abstract class FormBuilderComponentListingPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator) : Page<FormBuilderComponentViewerPageClientProperties>
{
    protected abstract string ComponentView { get; }

    public override async Task<FormBuilderComponentViewerPageClientProperties> ConfigureTemplateProperties(
        FormBuilderComponentViewerPageClientProperties properties)
    {
        properties.ComponentView = ComponentView;

        var model = await componentRegistryReadService.GetFormBuilderRegistryAsync();

        properties.FormComponents = model.FormComponents;
        properties.FormSections = model.FormSections;

        // Evaluate permissions and propagate to client
        var canViewFormBuilderUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_FORM_BUILDER_USAGES);
        properties.CanViewFormBuilderUsages = canViewFormBuilderUsages.Succeeded;

        return properties;
    }

    [PageCommand(CommandName = "GetFormBuilderComponentUsage", Permission = ComponentRegistryPermissions.VIEW_FORM_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetFormBuilderComponentUsage(
        ComponentDetailsParams @params)
    {
        if (@params?.ComponentIdentifier is null)
        {
            return ResponseFrom("Component identifier is required");
        }

        var usage = await componentUsageService.GetFormBuilderComponentUsageAsync(
            @params.ComponentIdentifier);

        AddAdminPaths(usage);

        return ResponseFrom(usage);
    }

    [PageCommand(CommandName = "GetFormBuilderSectionUsage", Permission = ComponentRegistryPermissions.VIEW_FORM_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetFormBuilderSectionUsage(
        ComponentDetailsParams @params)
    {
        if (@params?.ComponentIdentifier is null)
        {
            return ResponseFrom("Component identifier is required");
        }

        var usage = await componentUsageService.GetFormBuilderSectionUsageAsync(
            @params.ComponentIdentifier);

        AddAdminPaths(usage);

        return ResponseFrom(usage);
    }

    private void AddAdminPaths(FormComponentUsageDetailDto usage)
    {
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
    }
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
public class FormBuilderComponentsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : FormBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "components";
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
public class FormBuilderSectionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : FormBuilderComponentListingPage(pageLinkGenerator, componentRegistryReadService, componentUsageService, permissionEvaluator)
{
    protected override string ComponentView => "sections";
}

/// <summary>
/// Client properties for the form builder component viewer page.
/// </summary>
public class FormBuilderComponentViewerPageClientProperties : TemplateClientProperties
{
    public IEnumerable<FormComponentDto> FormComponents { get; set; } = [];
    public IEnumerable<FormSectionDto> FormSections { get; set; } = [];
    public bool CanViewFormBuilderUsages { get; set; }
    public string ComponentView { get; set; } = string.Empty;
}
