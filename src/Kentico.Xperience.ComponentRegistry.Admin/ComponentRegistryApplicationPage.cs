using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using Kentico.Xperience.ComponentRegistry.Admin;

[assembly: UIApplication(
    identifier: ComponentRegistryApplicationPage.IDENTIFIER,
    type: typeof(ComponentRegistryApplicationPage),
    slug: "component-registry",
    name: "Component Registry",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.CustomElement,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.ComponentRegistry.Admin;

/// <summary>
/// Application page for viewing registered components.
/// </summary>
[UIPermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER, displayName: "View Page Builder components")]
[UIPermission(ComponentRegistryPermissions.VIEW_PAGE_BUILDER_USAGES, displayName: "View Page Builder component usages")]
[UIPermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER, displayName: "View Form Builder components")]
[UIPermission(ComponentRegistryPermissions.VIEW_FORM_BUILDER_USAGES, displayName: "View Form Builder component usages")]
[UIPermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER, displayName: "View Email Builder components")]
[UIPermission(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER_USAGES, displayName: "View Email Builder component usages")]
[UIPermission(SystemPermissions.VIEW)]
public class ComponentRegistryApplicationPage : ApplicationPage
{
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
}
