using CMS.DataEngine;

using Kentico.EmailBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.Internal;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages.Internal;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

using Microsoft.Extensions.Options;

[assembly: UIPage(
    uiPageType: typeof(EmailBuilderStructurePage),
    parentType: typeof(EmailEditLayout),
    slug: BuilderStructureTab.SLUG,
    name: BuilderStructureTab.NAME,
    templateName: BuilderStructureTab.TEMPLATE_NAME,
    order: EmailBuilderStructurePage.ORDER,
    Icon = Icons.CustomElement)]

[assembly: PageExtender(typeof(EmailBuilderStructureNavigationExtender))]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// "Builder structure (Labs)" tab of an email: the Email Builder configuration of the latest version of the edited language variant
/// as nested boxes (email template, editable areas, sections, zones, widgets, widget variants).
/// Requires <see cref="ComponentRegistryPermissions.VIEW_EMAIL_BUILDER"/> in the Component Registry application, on top of the
/// email channel permissions that already guard <see cref="EmailEditLayout"/>.
/// </summary>
internal sealed class EmailBuilderStructurePage(
    IBuilderStructureService builderStructureService,
    IComponentRegistryPermissionEvaluator permissionEvaluator,
    IEmailConfigurationAdminModelRetriever emailConfigurationAdminModelRetriever,
    IOptionsMonitor<EmailBuilderOptions> emailBuilderOptions) : Page<BuilderStructureClientProperties>
{
    /// <summary>
    /// Directly after "email-builder" (<see cref="EmailBuilderTab"/>, 300) and before "statistics" (<see cref="EmailStatisticsTab"/>, 400).
    /// </summary>
    public const int ORDER = 350;

    private readonly IBuilderStructureService builderStructureService = builderStructureService;
    private readonly IComponentRegistryPermissionEvaluator permissionEvaluator = permissionEvaluator;
    private readonly IEmailConfigurationAdminModelRetriever emailConfigurationAdminModelRetriever = emailConfigurationAdminModelRetriever;
    private readonly IOptionsMonitor<EmailBuilderOptions> emailBuilderOptions = emailBuilderOptions;

    [PageParameter(typeof(IntPageModelBinder), typeof(EmailEditLayout))]
    public int EmailConfigurationID { get; set; }

    /// <summary>
    /// Content language code name. <see cref="EmailEditLayout"/> validates it.
    /// </summary>
    [PageParameter(typeof(StringPageModelBinder), typeof(EmailChannelContentLanguage))]
    public string LanguageName { get; set; } = string.Empty;

    public override async Task<PageValidationResult> ValidatePage()
    {
        await permissionEvaluator.EnsureGranted(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER);

        string? errorMessageKey = await GetEmailBuilderUnavailableMessageKey(
            emailConfigurationAdminModelRetriever,
            emailBuilderOptions,
            EmailConfigurationID,
            LanguageName);

        return errorMessageKey is null
            ? await base.ValidatePage()
            : new() { IsValid = false, ErrorMessageKey = errorMessageKey };
    }

    public override async Task<BuilderStructureClientProperties> ConfigureTemplateProperties(BuilderStructureClientProperties properties)
    {
        await permissionEvaluator.EnsureGranted(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER);

        properties.Builder = BuilderStructureBuilders.EMAIL_BUILDER;
        properties.LanguageName = LanguageName;
        properties.Structure = await builderStructureService.GetEmailStructureAsync(EmailConfigurationID, LanguageName);

        return properties;
    }

    /// <summary>
    /// Returns the localization key of the reason the email cannot use Email Builder, or <c>null</c> when it can.
    /// Mirrors <see cref="EmailBuilderTab"/>: emails based on a legacy email template and emails of content types not registered for Email Builder don't use it.
    /// </summary>
    internal static async Task<string?> GetEmailBuilderUnavailableMessageKey(
        IEmailConfigurationAdminModelRetriever emailConfigurationAdminModelRetriever,
        IOptionsMonitor<EmailBuilderOptions> emailBuilderOptions,
        int emailConfigurationId,
        string languageName)
    {
        var email = await emailConfigurationAdminModelRetriever.Get(emailConfigurationId, languageName);

        if (email.TemplateID > 0)
        {
            return "digitalmarketing.emails.emailbuilderdisabled.emailtemplatedoesnotsupportbuilder";
        }

        // Same check as the product's internal IEmailBuilderSupportingContentTypesArbiter
        string? contentTypeName = DataClassInfoProvider.GetDataClassInfo(email.ContentTypeID)?.ClassName;

        return contentTypeName is not null && emailBuilderOptions.CurrentValue.AllowedEmailContentTypeNames.Contains(contentTypeName, StringComparer.Ordinal)
            ? null
            : "digitalmarketing.emails.emailbuilderdisabled.contenttypeisnotallowed";
    }
}

/// <summary>
/// Hides the "Builder structure (Labs)" tab for emails that don't use Email Builder and for users without
/// <see cref="ComponentRegistryPermissions.VIEW_EMAIL_BUILDER"/>. The page itself checks both too.
/// </summary>
internal sealed class EmailBuilderStructureNavigationExtender(
    IComponentRegistryPermissionEvaluator permissionEvaluator,
    IEmailConfigurationAdminModelRetriever emailConfigurationAdminModelRetriever,
    IOptionsMonitor<EmailBuilderOptions> emailBuilderOptions) : PageExtender<EmailEditLayout>
{
    private readonly IComponentRegistryPermissionEvaluator permissionEvaluator = permissionEvaluator;
    private readonly IEmailConfigurationAdminModelRetriever emailConfigurationAdminModelRetriever = emailConfigurationAdminModelRetriever;
    private readonly IOptionsMonitor<EmailBuilderOptions> emailBuilderOptions = emailBuilderOptions;

    public override async Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
    {
        properties = await base.ConfigureTemplateProperties(properties);

        if (!await permissionEvaluator.IsGranted(ComponentRegistryPermissions.VIEW_EMAIL_BUILDER)
            || await EmailBuilderStructurePage.GetEmailBuilderUnavailableMessageKey(
                emailConfigurationAdminModelRetriever,
                emailBuilderOptions,
                Page.EmailConfigurationID,
                Page.ContentLanguageIdentifier.LanguageName) is not null)
        {
            properties.Navigation.Items = [.. properties.Navigation.Items
                .Where(item => !string.Equals(item.Path, BuilderStructureTab.SLUG, StringComparison.OrdinalIgnoreCase))];
        }

        return properties;
    }
}
