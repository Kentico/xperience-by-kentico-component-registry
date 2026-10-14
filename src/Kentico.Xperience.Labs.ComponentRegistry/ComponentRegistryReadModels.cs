namespace Kentico.Xperience.Labs.ComponentRegistry;

/// <summary>
/// Parameters for requesting usage details by component identifier.
/// </summary>
public record ComponentDetailsParams(string ComponentIdentifier);

/// <summary>
/// Data transfer object for component definitions.
/// </summary>
public record ComponentDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName);

/// <summary>
/// Data transfer object for page template definitions.
/// </summary>
public record PageTemplateDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName,
    string[] ContentTypeNames);

/// <summary>
/// Data transfer object for email builder component definitions.
/// </summary>
public record EmailComponentDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName,
    string? PropertiesTypeName);

/// <summary>
/// Data transfer object for email builder template definitions.
/// </summary>
public record EmailTemplateDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName,
    string[] ContentTypeNames);

/// <summary>
/// Data transfer object for form builder component definitions.
/// </summary>
public record FormComponentDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName);

/// <summary>
/// Data transfer object for Admin UI form component definitions.
/// </summary>
public record AdminFormComponentDto(
    string Identifier,
    string Name,
    string? MarkedTypeName,
    bool IsAvailable);

/// <summary>
/// Data transfer object for form builder section definitions.
/// </summary>
public record FormSectionDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName);

/// <summary>
/// Data transfer object for custom automation action and condition definitions.
/// </summary>
/// <param name="Identifier">Step identifier stored in the automation process step parameters.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Description">Localized description.</param>
/// <param name="IconName">Admin icon name.</param>
/// <param name="ImplementationTypeName">Full name of the action or condition implementation type.</param>
/// <param name="PropertiesTypeName">Full name of the step properties type, if any.</param>
public record AutomationStepDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconName,
    string? ImplementationTypeName,
    string? PropertiesTypeName);

/// <summary>
/// Data transfer object for custom automation trigger definitions.
/// </summary>
/// <param name="Identifier">Trigger identifier stored in the automation process trigger parameters.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Description">Localized description.</param>
/// <param name="IconName">Admin icon name.</param>
/// <param name="ImplementationTypeName">Full name of the trigger implementation type.</param>
/// <param name="PropertiesTypeName">Full name of the trigger properties type, if any.</param>
/// <param name="TriggerDataTypeName">Full name of the trigger data type, if any.</param>
/// <param name="ObjectType">Object type the trigger is bound to.</param>
public record AutomationTriggerDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconName,
    string? ImplementationTypeName,
    string? PropertiesTypeName,
    string? TriggerDataTypeName,
    string? ObjectType);

/// <summary>
/// Aggregated read model for page builder registry definitions.
/// </summary>
public record PageBuilderRegistryReadModel(
    IReadOnlyList<ComponentDto> Widgets,
    IReadOnlyList<ComponentDto> Sections,
    IReadOnlyList<PageTemplateDto> PageTemplates);

/// <summary>
/// Aggregated read model for email builder registry definitions.
/// </summary>
public record EmailBuilderRegistryReadModel(
    IReadOnlyList<EmailComponentDto> Widgets,
    IReadOnlyList<EmailComponentDto> Sections,
    IReadOnlyList<EmailTemplateDto> EmailTemplates);

/// <summary>
/// Aggregated read model for form builder registry definitions.
/// </summary>
public record FormBuilderRegistryReadModel(
    IReadOnlyList<FormComponentDto> FormComponents,
    IReadOnlyList<FormSectionDto> FormSections);

/// <summary>
/// Aggregated read model for Admin UI form component definitions.
/// </summary>
public record AdminFormComponentRegistryReadModel(
    IReadOnlyList<AdminFormComponentDto> FormComponents);

/// <summary>
/// Aggregated read model for custom automation step definitions.
/// </summary>
public record AutomationRegistryReadModel(
    IReadOnlyList<AutomationTriggerDto> Triggers,
    IReadOnlyList<AutomationStepDto> Actions,
    IReadOnlyList<AutomationStepDto> Conditions);
