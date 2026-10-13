using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.DigitalMarketing.UIPages.Internal;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

[assembly: UIPage(
    uiPageType: typeof(AutomationBuilderComponentViewerPage),
    parentType: typeof(ComponentRegistryApplicationPage),
    slug: "automation-builder",
    name: "Automation Builder",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 4,
    Icon = Icons.ProcessScheme)]
[assembly: UIPage(
    uiPageType: typeof(AutomationBuilderTriggersPage),
    parentType: typeof(AutomationBuilderComponentViewerPage),
    slug: "triggers",
    name: "Triggers",
    templateName: AutomationBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 1)]
[assembly: UIPage(
    uiPageType: typeof(AutomationBuilderActionsPage),
    parentType: typeof(AutomationBuilderComponentViewerPage),
    slug: "actions",
    name: "Actions",
    templateName: AutomationBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 2)]
[assembly: UIPage(
    uiPageType: typeof(AutomationBuilderConditionsPage),
    parentType: typeof(AutomationBuilderComponentViewerPage),
    slug: "conditions",
    name: "Conditions",
    templateName: AutomationBuilderComponentViewerPage.TEMPLATE_NAME,
    order: 3)]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Automation Builder section. Redirects to its first child page.
/// </summary>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER)]
[UINavigation(true)]
public class AutomationBuilderComponentViewerPage : SecondaryMenuSectionPage
{
    public const string TEMPLATE_NAME = "@kentico/xperience-labs-component-registry-admin/AutomationBuilderComponentViewer";
}

/// <summary>
/// Base page for displaying registered custom automation steps of a single kind.
/// Each page sends only its own step definitions to the client.
/// </summary>
/// <typeparam name="TStep">Step definition DTO sent to the client.</typeparam>
[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER)]
public abstract class AutomationBuilderComponentListingPage<TStep>(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IUIPermissionEvaluator permissionEvaluator) : Page<AutomationBuilderComponentViewerPageClientProperties<TStep>>
{
    protected abstract AutomationStepKind StepKind { get; }

    protected abstract IReadOnlyList<TStep> GetSteps(AutomationRegistryReadModel model);

    protected abstract Task<AutomationStepUsageDetailDto> GetUsage(string identifier);

    public override async Task<AutomationBuilderComponentViewerPageClientProperties<TStep>> ConfigureTemplateProperties(
        AutomationBuilderComponentViewerPageClientProperties<TStep> properties)
    {
        var model = await componentRegistryReadService.GetAutomationRegistryAsync();

        properties.StepKind = StepKind.ToString().ToLowerInvariant();
        properties.Steps = GetSteps(model);

        var canViewUsages = await permissionEvaluator.Evaluate(
            ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER_USAGES);
        properties.CanViewUsages = canViewUsages.Succeeded;

        return properties;
    }

    [PageCommand(CommandName = "GetAutomationStepUsage", Permission = ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER_USAGES)]
    public async Task<ICommandResponse> GetAutomationStepUsage(ComponentDetailsParams @params)
    {
        if (@params?.ComponentIdentifier is null)
        {
            return ResponseFrom("Component identifier is required");
        }

        var usage = await GetUsage(@params.ComponentIdentifier);

        foreach (var process in usage.Processes)
        {
            string adminPath = pageLinkGenerator.GetPath<AutomationProcessBuilder>(
                new PageParameterValues()
                {
                    { typeof(AutomationProcessEditSection), process.ProcessID.ToString() },
                });

            process.AdminPath = adminPath.StartsWith('/')
                ? adminPath[1..]
                : adminPath;
        }

        return ResponseFrom(usage);
    }
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER)]
public class AutomationBuilderTriggersPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : AutomationBuilderComponentListingPage<AutomationTriggerDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override AutomationStepKind StepKind => AutomationStepKind.Trigger;

    protected override IReadOnlyList<AutomationTriggerDto> GetSteps(AutomationRegistryReadModel model) => model.Triggers;

    protected override Task<AutomationStepUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetAutomationTriggerUsageAsync(identifier);
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER)]
public class AutomationBuilderActionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : AutomationBuilderComponentListingPage<AutomationStepDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override AutomationStepKind StepKind => AutomationStepKind.Action;

    protected override IReadOnlyList<AutomationStepDto> GetSteps(AutomationRegistryReadModel model) => model.Actions;

    protected override Task<AutomationStepUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetAutomationActionUsageAsync(identifier);
}

[UIEvaluatePermission(ComponentRegistryPermissions.VIEW_AUTOMATION_BUILDER)]
public class AutomationBuilderConditionsPage(
    IPageLinkGenerator pageLinkGenerator,
    IComponentRegistryReadService componentRegistryReadService,
    IComponentUsageService componentUsageService,
    IUIPermissionEvaluator permissionEvaluator)
    : AutomationBuilderComponentListingPage<AutomationStepDto>(pageLinkGenerator, componentRegistryReadService, permissionEvaluator)
{
    protected override AutomationStepKind StepKind => AutomationStepKind.Condition;

    protected override IReadOnlyList<AutomationStepDto> GetSteps(AutomationRegistryReadModel model) => model.Conditions;

    protected override Task<AutomationStepUsageDetailDto> GetUsage(string identifier) =>
        componentUsageService.GetAutomationConditionUsageAsync(identifier);
}

/// <summary>
/// Client properties for an automation builder step listing page.
/// </summary>
/// <typeparam name="TStep">Step definition DTO.</typeparam>
public class AutomationBuilderComponentViewerPageClientProperties<TStep> : TemplateClientProperties
{
    public IReadOnlyList<TStep> Steps { get; set; } = [];
    public string StepKind { get; set; } = string.Empty;
    public bool CanViewUsages { get; set; }
}
