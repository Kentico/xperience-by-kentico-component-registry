namespace Kentico.Xperience.Labs.ComponentRegistry.Tests;

internal sealed class StubReadService(
    PageBuilderRegistryReadModel page,
    EmailBuilderRegistryReadModel email,
    FormBuilderRegistryReadModel form,
    AutomationRegistryReadModel? automation = null) : IComponentRegistryReadService
{
    public Task<AutomationRegistryReadModel> GetAutomationRegistryAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(automation ?? new AutomationRegistryReadModel([], [], []));

    public Task<PageBuilderRegistryReadModel> GetPageBuilderRegistryAsync(CancellationToken cancellationToken = default) => Task.FromResult(page);

    public Task<EmailBuilderRegistryReadModel> GetEmailBuilderRegistryAsync(CancellationToken cancellationToken = default) => Task.FromResult(email);

    public Task<FormBuilderRegistryReadModel> GetFormBuilderRegistryAsync(CancellationToken cancellationToken = default) => Task.FromResult(form);
}

internal sealed class StubUsageService : IComponentUsageService
{
    public string? LastCall { get; private set; }

    public Task<ComponentUsageDetailDto> GetPageBuilderPageTemplateUsageAsync(string templateIdentifier)
    {
        LastCall = $"page-template:{templateIdentifier}";
        return Task.FromResult(CreatePageUsage(templateIdentifier, "PageTemplate"));
    }

    public Task<ComponentUsageDetailDto> GetPageBuilderWidgetUsageAsync(string widgetIdentifier)
    {
        LastCall = $"page-widget:{widgetIdentifier}";
        return Task.FromResult(CreatePageUsage(widgetIdentifier, "Widget"));
    }

    public Task<ComponentUsageDetailDto> GetPageBuilderSectionUsageAsync(string sectionIdentifier)
    {
        LastCall = $"page-section:{sectionIdentifier}";
        return Task.FromResult(new ComponentUsageDetailDto { ComponentIdentifier = sectionIdentifier, ComponentType = "Section" });
    }

    public Task<List<ComponentUsageDetailDto>> GetBatchUsageAsync(List<string> identifiers, string componentType)
    {
        LastCall = $"batch:{componentType}:{identifiers.Count}";
        return Task.FromResult(identifiers.Select(i => CreatePageUsage(i, componentType)).ToList());
    }

    public Task<EmailConfigurationUsageDetailDto> GetEmailBuilderWidgetUsageAsync(string widgetIdentifier)
    {
        LastCall = $"email-widget:{widgetIdentifier}";
        return Task.FromResult(new EmailConfigurationUsageDetailDto { ComponentIdentifier = widgetIdentifier, ComponentType = "EmailWidget" });
    }

    public Task<EmailConfigurationUsageDetailDto> GetEmailBuilderSectionUsageAsync(string sectionIdentifier)
    {
        LastCall = $"email-section:{sectionIdentifier}";
        return Task.FromResult(new EmailConfigurationUsageDetailDto { ComponentIdentifier = sectionIdentifier, ComponentType = "EmailSection" });
    }

    public Task<EmailConfigurationUsageDetailDto> GetEmailBuilderTemplateUsageAsync(string templateIdentifier)
    {
        LastCall = $"email-template:{templateIdentifier}";
        return Task.FromResult(new EmailConfigurationUsageDetailDto { ComponentIdentifier = templateIdentifier, ComponentType = "EmailTemplate" });
    }

    public Task<FormComponentUsageDetailDto> GetFormBuilderComponentUsageAsync(string componentIdentifier)
    {
        LastCall = $"form-component:{componentIdentifier}";
        return Task.FromResult(new FormComponentUsageDetailDto { ComponentIdentifier = componentIdentifier, ComponentType = "Component" });
    }

    public Task<FormComponentUsageDetailDto> GetFormBuilderSectionUsageAsync(string sectionIdentifier)
    {
        LastCall = $"form-section:{sectionIdentifier}";
        return Task.FromResult(new FormComponentUsageDetailDto { ComponentIdentifier = sectionIdentifier, ComponentType = "Section" });
    }

    public Task<AutomationStepUsageDetailDto> GetAutomationTriggerUsageAsync(string triggerIdentifier)
    {
        LastCall = $"automation-trigger:{triggerIdentifier}";
        return Task.FromResult(new AutomationStepUsageDetailDto { ComponentIdentifier = triggerIdentifier, ComponentType = "Trigger" });
    }

    public Task<AutomationStepUsageDetailDto> GetAutomationActionUsageAsync(string actionIdentifier)
    {
        LastCall = $"automation-action:{actionIdentifier}";
        return Task.FromResult(new AutomationStepUsageDetailDto { ComponentIdentifier = actionIdentifier, ComponentType = "Action" });
    }

    public Task<AutomationStepUsageDetailDto> GetAutomationConditionUsageAsync(string conditionIdentifier)
    {
        LastCall = $"automation-condition:{conditionIdentifier}";
        return Task.FromResult(new AutomationStepUsageDetailDto { ComponentIdentifier = conditionIdentifier, ComponentType = "Condition" });
    }

    private static ComponentUsageDetailDto CreatePageUsage(string identifier, string componentType) =>
        new()
        {
            ComponentIdentifier = identifier,
            ComponentType = componentType,
            TotalPagesUsing = 1,
            TotalVariants = 1,
            Pages =
            [
                new PageUsageDto
                {
                    WebPageItemId = 42,
                    ContentItemId = 84,
                    PageName = "Test page",
                    PagePath = "/test-page",
                    WebsiteChannelID = 7,
                    ChannelDisplayName = "Website",
                    ContentTypeDisplayName = "Article",
                    Variants =
                    [
                        new PageVariantDto
                        {
                            ContentItemCommonDataId = 21,
                            LanguageName = "en-US",
                            ConfigurationJson = "{}",
                            ConfigurationType = componentType,
                            IsPublished = true
                        }
                    ]
                }
            ]
        };
}
