namespace Kentico.Xperience.ComponentRegistry.Tests;

internal sealed class StubReadService(
    PageBuilderRegistryReadModel page,
    EmailBuilderRegistryReadModel email,
    FormBuilderRegistryReadModel form) : IComponentRegistryReadService
{
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
