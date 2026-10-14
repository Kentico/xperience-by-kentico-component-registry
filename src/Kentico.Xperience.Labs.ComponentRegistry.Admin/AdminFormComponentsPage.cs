using Kentico.Xperience.Admin.Base;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

[assembly: UIPage(
    uiPageType: typeof(AdminFormComponentsPage),
    parentType: typeof(ComponentRegistryApplicationPage),
    slug: "form-components",
    name: "Form Components",
    templateName: AdminFormComponentsPage.TEMPLATE_NAME,
    order: 5,
    Icon = Icons.Form)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Page listing Admin UI form components registered with the Admin Base <c>RegisterFormComponentAttribute</c>
/// and the persisted class fields that use them. Separate from Form Builder components.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_FORM_COMPONENTS)]
public class AdminFormComponentsPage(
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator) : Page<AdminFormComponentsPageClientProperties>
{
    public const string TEMPLATE_NAME = "@kentico/xperience-labs-component-registry-admin/AdminFormComponentViewer";

    public override async Task<AdminFormComponentsPageClientProperties> ConfigureTemplateProperties(
        AdminFormComponentsPageClientProperties properties)
    {
        var model = await componentRegistryReadService.GetAdminFormComponentRegistryAsync();

        properties.Components = model.FormComponents.Where(c => c.IsAvailable).ToList();

        var canViewUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_FORM_COMPONENTS_USAGES);
        properties.CanViewUsages = canViewUsages.Succeeded;

        return properties;
    }

    [PageCommand(CommandName = "GetAdminFormComponentUsage", Permission = ComponentRegistryPermissions.VIEW_FORM_COMPONENTS_USAGES)]
    public async Task<ICommandResponse> GetAdminFormComponentUsage(ComponentDetailsParams @params)
    {
        if (@params?.ComponentIdentifier is null)
        {
            return ResponseFrom("Component identifier is required");
        }

        return ResponseFrom(await componentUsageService.GetAdminFormComponentUsageAsync(@params.ComponentIdentifier));
    }
}

/// <summary>
/// Client properties for the Admin UI form components page.
/// </summary>
public class AdminFormComponentsPageClientProperties : TemplateClientProperties
{
    public IReadOnlyList<AdminFormComponentDto> Components { get; set; } = [];
    public bool CanViewUsages { get; set; }
}
