namespace Kentico.Xperience.ComponentRegistry;

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
/// Data transfer object for form builder section definitions.
/// </summary>
public record FormSectionDto(
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName);

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
