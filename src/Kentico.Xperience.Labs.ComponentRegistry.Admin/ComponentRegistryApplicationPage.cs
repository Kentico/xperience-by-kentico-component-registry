using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

[assembly: UIApplication(
    identifier: ComponentRegistryApplicationPage.IDENTIFIER,
    type: typeof(ComponentRegistryApplicationPage),
    slug: "component-registry",
    name: "Component Registry (Labs)",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.CustomElement,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Application page for viewing registered components.
/// </summary>
[UIPermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER, displayName: "View Page Builder components")]
[UIPermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER_USAGES, displayName: "View Page Builder component usages")]
[UIPermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER, displayName: "View Form Builder components")]
[UIPermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER_USAGES, displayName: "View Form Builder component usages")]
[UIPermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER, displayName: "View Email Builder components")]
[UIPermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER_USAGES, displayName: "View Email Builder component usages")]
[UIPermission(ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER, displayName: "View Automation Builder components")]
[UIPermission(ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER_USAGES, displayName: "View Automation Builder component usages")]
[UIPermission(ComponentRegistryPermissions.VIEW_FORM_COMPONENTS, displayName: "View Form Components")]
[UIPermission(ComponentRegistryPermissions.VIEW_FORM_COMPONENTS_USAGES, displayName: "View Form Component usages")]
[UIPermission(SystemPermissions.VIEW)]
public class ComponentRegistryApplicationPage : ApplicationPage
{
    // Pre-rename value kept: role permissions are stored against this identifier
    public const string IDENTIFIER = "Kentico.Xperience.ComponentRegistry.Admin.App";
}

public static class ComponentRegistryPermissions
{
    public const string VIEW_PAGE_BUILDER = "View_Page_Builder";
    public const string VIEW_PAGE_BUILDER_USAGES = "View_Page_Builder_Usages";
    public const string VIEW_FORM_BUILDER = "View_Form_Builder";
    public const string VIEW_FORM_BUILDER_USAGES = "View_Form_Builder_Usages";
    public const string VIEW_EMAIL_BUILDER = "View_Email_Builder";
    public const string VIEW_EMAIL_BUILDER_USAGES = "View_Email_Builder_Usages";
    public const string VIEW_AUTOMATION_BUILDER = "View_Automation_Builder";
    public const string VIEW_AUTOMATION_BUILDER_USAGES = "View_Automation_Builder_Usages";
    public const string VIEW_FORM_COMPONENTS = "View_Form_Components";
    public const string VIEW_FORM_COMPONENTS_USAGES = "View_Form_Components_Usages";
}
