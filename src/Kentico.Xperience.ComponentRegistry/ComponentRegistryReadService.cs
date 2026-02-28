namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Read-only service exposing normalized component registry definitions.
/// </summary>
public interface IComponentRegistryReadService
{
    /// <summary>
    /// Gets all page builder component definitions.
    /// </summary>
    public Task<PageBuilderRegistryReadModel> GetPageBuilderRegistryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all email builder component definitions.
    /// </summary>
    public Task<EmailBuilderRegistryReadModel> GetEmailBuilderRegistryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all form builder component definitions.
    /// </summary>
    public Task<FormBuilderRegistryReadModel> GetFormBuilderRegistryAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IComponentRegistryReadService"/>.
/// </summary>
public class ComponentRegistryReadService(
    IComponentDefinitionStore<PageBuilderWidgetDefinition> pageBuilderWidgetStore,
    IComponentDefinitionStore<PageBuilderSectionDefinition> pageBuilderSectionStore,
    IComponentDefinitionStore<PageBuilderPageTemplateDefinition> pageBuilderPageTemplateStore,
    IComponentDefinitionStore<EmailBuilderWidgetDefinition> emailBuilderWidgetStore,
    IComponentDefinitionStore<EmailBuilderSectionDefinition> emailBuilderSectionStore,
    IComponentDefinitionStore<EmailBuilderTemplateDefinition> emailBuilderTemplateStore,
    IComponentDefinitionStore<FormBuilderComponentDefinition> formBuilderComponentStore,
    IComponentDefinitionStore<FormBuilderSectionDefinition> formBuilderSectionStore,
    IComponentRegistryLocalizationService localizationService) : IComponentRegistryReadService
{
    /// <inheritdoc/>
    public Task<PageBuilderRegistryReadModel> GetPageBuilderRegistryAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var widgets = pageBuilderWidgetStore.GetAll()
            .Select(w => new ComponentDto(
                w.Identifier,
                localizationService.Localize(w.Name),
                LocalizeOrNull(w.Description),
                w.IconClass,
                w.MarkedType?.FullName))
            .ToList();

        var sections = pageBuilderSectionStore.GetAll()
            .Select(s => new ComponentDto(
                s.Identifier,
                localizationService.Localize(s.Name),
                LocalizeOrNull(s.Description),
                s.IconClass,
                s.MarkedType?.FullName))
            .ToList();

        var pageTemplates = pageBuilderPageTemplateStore.GetAll()
            .Select(pt => new PageTemplateDto(
                pt.Identifier,
                localizationService.Localize(pt.Name),
                LocalizeOrNull(pt.Description),
                pt.IconClass,
                pt.MarkedType?.FullName,
                pt.ContentTypeNames))
            .ToList();

        return Task.FromResult(new PageBuilderRegistryReadModel(widgets, sections, pageTemplates));
    }

    /// <inheritdoc/>
    public Task<EmailBuilderRegistryReadModel> GetEmailBuilderRegistryAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var widgets = emailBuilderWidgetStore.GetAll()
            .Select(w => new EmailComponentDto(
                w.Identifier,
                localizationService.Localize(w.Name),
                LocalizeOrNull(w.Description),
                w.IconClass,
                w.MarkedType?.FullName,
                w.PropertiesType?.FullName))
            .ToList();

        var sections = emailBuilderSectionStore.GetAll()
            .Select(s => new EmailComponentDto(
                s.Identifier,
                localizationService.Localize(s.Name),
                LocalizeOrNull(s.Description),
                s.IconClass,
                s.MarkedType?.FullName,
                null))
            .ToList();

        var templates = emailBuilderTemplateStore.GetAll()
            .Select(t => new EmailTemplateDto(
                t.Identifier,
                localizationService.Localize(t.Name),
                LocalizeOrNull(t.Description),
                t.IconClass,
                t.MarkedType?.FullName,
                t.ContentTypeNames))
            .ToList();

        return Task.FromResult(new EmailBuilderRegistryReadModel(widgets, sections, templates));
    }

    /// <inheritdoc/>
    public Task<FormBuilderRegistryReadModel> GetFormBuilderRegistryAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var components = formBuilderComponentStore.GetAll()
            .Select(c => new FormComponentDto(
                c.Identifier,
                localizationService.Localize(c.Name),
                LocalizeOrNull(c.Description),
                c.IconClass,
                c.MarkedType?.FullName))
            .ToList();

        var sections = formBuilderSectionStore.GetAll()
            .Select(s => new FormSectionDto(
                s.Identifier,
                localizationService.Localize(s.Name),
                LocalizeOrNull(s.Description),
                s.IconClass,
                s.MarkedType?.FullName))
            .ToList();

        return Task.FromResult(new FormBuilderRegistryReadModel(components, sections));
    }

    private string? LocalizeOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? value
            : localizationService.Localize(value);
}
