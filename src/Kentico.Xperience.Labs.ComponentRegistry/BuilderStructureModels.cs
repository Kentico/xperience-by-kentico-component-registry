namespace Kentico.Xperience.Labs.ComponentRegistry;

/// <summary>
/// Values of <see cref="BuilderNodeDto.Kind"/>.
/// </summary>
public static class BuilderNodeKinds
{
    /// <summary>
    /// Root of a web page or email that stores widgets but no template configuration.
    /// </summary>
    public const string ROOT = "root";
    public const string TEMPLATE = "template";
    public const string EDITABLE_AREA = "editableArea";
    public const string SECTION = "section";
    public const string ZONE = "zone";
    public const string WIDGET = "widget";
    public const string VARIANT = "variant";
}

/// <summary>
/// Property of a Page Builder or Email Builder component instance. <see cref="Value"/> is the raw text of scalars or compact JSON of objects and arrays.
/// </summary>
public record BuilderPropertyDto(string Name, string Value);

/// <summary>
/// Node of a Page Builder or Email Builder structure: template → editable areas → sections → zones → widgets → widget variants.
/// Both builders store the same JSON shape.
/// </summary>
public record BuilderNodeDto
{
    /// <summary>
    /// One of <see cref="BuilderNodeKinds"/>.
    /// </summary>
    public required string Kind { get; init; }

    /// <summary>
    /// Editable area identifier, zone name, or variant name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Identifier of the registered component (page or email template, section, or widget type).
    /// </summary>
    public string? ComponentIdentifier { get; init; }

    /// <summary>
    /// Registered display name of <see cref="ComponentIdentifier"/>.
    /// </summary>
    public string? ComponentName { get; init; }

    public string? IconClass { get; init; }

    /// <summary>
    /// Whether <see cref="ComponentIdentifier"/> is registered. <c>null</c> for nodes that are not components.
    /// </summary>
    public bool? IsRegistered { get; init; }

    /// <summary>
    /// Instance GUID of a section, zone, widget, or variant.
    /// </summary>
    public string? InstanceId { get; init; }

    /// <summary>
    /// Pre-configured page template the page was created from.
    /// </summary>
    public string? ConfigurationId { get; init; }

    /// <summary>
    /// Personalization condition type identifier of a widget.
    /// </summary>
    public string? ConditionType { get; init; }

    public IReadOnlyList<BuilderPropertyDto> Properties { get; init; } = [];

    /// <summary>
    /// Personalization condition parameters of a widget variant.
    /// </summary>
    public IReadOnlyList<BuilderPropertyDto> ConditionParameters { get; init; } = [];

    public IReadOnlyList<BuilderNodeDto> Children { get; init; } = [];
}

/// <summary>
/// Page Builder structure of one web page language variant, or Email Builder structure of one email language variant.
/// </summary>
public record BuilderStructureDto
{
    public required string LanguageName { get; init; }

    public required string LanguageDisplayName { get; init; }

    /// <summary>
    /// <see cref="CMS.ContentEngine.VersionStatus"/> name of the latest version, which is the one shown.
    /// </summary>
    public required string VersionStatus { get; init; }

    /// <summary>
    /// Whether the latest version differs from the published version. <c>null</c> when the latest version is the published one or nothing is published.
    /// </summary>
    public bool? DiffersFromPublished { get; init; }

    /// <summary>
    /// Template, or a <see cref="BuilderNodeKinds.ROOT"/> node when there are widgets but no template. <c>null</c> when there is no builder data.
    /// </summary>
    public BuilderNodeDto? Root { get; init; }

    /// <summary>
    /// Pretty-printed editable areas (widgets) JSON.
    /// </summary>
    public string? WidgetsJson { get; init; }

    /// <summary>
    /// Pretty-printed page or email template configuration JSON.
    /// </summary>
    public string? TemplateConfigurationJson { get; init; }

    /// <summary>
    /// Parts of the stored JSON that could not be read.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];
}

/// <summary>
/// Page Builder or Email Builder data of one content item common data row.
/// </summary>
/// <param name="VersionStatus"><see cref="CMS.ContentEngine.VersionStatus"/> value.</param>
/// <param name="WidgetsJson"><c>ContentItemCommonDataVisualBuilderWidgets</c>.</param>
/// <param name="TemplateConfigurationJson"><c>ContentItemCommonDataVisualBuilderTemplateConfiguration</c>.</param>
public record BuilderDataVersion(int VersionStatus, string? WidgetsJson, string? TemplateConfigurationJson);
