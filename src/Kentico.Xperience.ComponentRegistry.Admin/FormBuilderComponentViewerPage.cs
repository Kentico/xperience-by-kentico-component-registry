using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;

using Kentico.Xperience.ComponentRegistry.Admin;

[assembly: UIPage(
    uiPageType: typeof(FormBuilderComponentViewerPage),
    parentType: typeof(ComponentRegistryApplicationPage),
    slug: "form-builder",
    name: "Form Builder",
    templateName: "@kentico/xperience-integrations-component-registry-web-admin/FormBuilderComponentViewer",
    order: 3,
    Icon = Icons.CustomElement)]

namespace Kentico.Xperience.ComponentRegistry.Admin;

/// <summary>
/// Page for displaying all registered form builder component definitions.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER)]
public class FormBuilderComponentViewerPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator) : Page<FormBuilderComponentViewerPageClientProperties>
{
    public override async Task<FormBuilderComponentViewerPageClientProperties> ConfigureTemplateProperties(
        FormBuilderComponentViewerPageClientProperties properties)
    {
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

/// <summary>
/// Client properties for the form builder component viewer page.
/// </summary>
public class FormBuilderComponentViewerPageClientProperties : TemplateClientProperties
{
    public IEnumerable<FormComponentDto> FormComponents { get; set; } = [];
    public IEnumerable<FormSectionDto> FormSections { get; set; } = [];
    public bool CanViewFormBuilderUsages { get; set; }
}
