using System.Text.Encodings.Web;
using System.Text.Json;

using CMS.ContentEngine;

namespace Kentico.Xperience.Labs.ComponentRegistry;

/// <summary>
/// Builds the <see cref="BuilderStructureDto"/> of a web page or email from its stored Page Builder or Email Builder JSON.
/// Both builders store the same JSON shape.
/// </summary>
internal static class BuilderStructureParser
{
    // Relaxed escaping keeps HTML and non-ASCII property values readable.
    private static readonly JsonSerializerOptions compactOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static readonly JsonSerializerOptions indentedOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };

    /// <summary>
    /// Creates the structure of the <paramref name="latest"/> version. <paramref name="published"/> is the published version when it is a different row.
    /// </summary>
    public static BuilderStructureDto Create(
        string languageName,
        string languageDisplayName,
        BuilderDataVersion latest,
        BuilderDataVersion? published,
        BuilderComponentLookup lookup)
    {
        var errors = new List<string>();
        var root = Parse(latest.WidgetsJson, latest.TemplateConfigurationJson, lookup, errors);

        return new BuilderStructureDto
        {
            LanguageName = languageName,
            LanguageDisplayName = languageDisplayName,
            VersionStatus = ((VersionStatus)latest.VersionStatus).ToString(),
            DiffersFromPublished = published is null
                ? null
                : !string.Equals(Normalize(latest.WidgetsJson), Normalize(published.WidgetsJson), StringComparison.Ordinal)
                    || !string.Equals(Normalize(latest.TemplateConfigurationJson), Normalize(published.TemplateConfigurationJson), StringComparison.Ordinal),
            Root = root,
            WidgetsJson = Format(latest.WidgetsJson, indentedOptions),
            TemplateConfigurationJson = Format(latest.TemplateConfigurationJson, indentedOptions),
            Errors = errors,
        };
    }

    /// <summary>
    /// Returns the template node (or a <see cref="BuilderNodeKinds.ROOT"/> node when there is no template) with the editable areas as children,
    /// or <c>null</c> when there is neither a template nor an editable area.
    /// </summary>
    public static BuilderNodeDto? Parse(
        string? widgetsJson,
        string? templateJson,
        BuilderComponentLookup lookup,
        ICollection<string> errors)
    {
        var areas = ParseAreas(widgetsJson, lookup, errors);

        using var template = TryParse(templateJson, "template configuration", errors);
        if (template?.RootElement.ValueKind == JsonValueKind.Object)
        {
            var element = template.RootElement;
            string? identifier = GetString(element, "identifier");
            var definition = lookup.FindTemplate(identifier);
            string? configurationId = GetString(element, "configurationIdentifier");
            bool hasConfigurationId = configurationId is not null && !(Guid.TryParse(configurationId, out var id) && id == Guid.Empty);

            return new BuilderNodeDto
            {
                Kind = BuilderNodeKinds.TEMPLATE,
                ComponentIdentifier = identifier,
                ComponentName = definition?.Name,
                IconClass = definition?.IconClass,
                IsRegistered = definition is not null,
                ConfigurationId = hasConfigurationId ? configurationId : null,
                Properties = GetProperties(element, "properties"),
                Children = areas,
            };
        }

        return areas.Count > 0
            ? new BuilderNodeDto { Kind = BuilderNodeKinds.ROOT, Children = areas }
            : null;
    }

    private static List<BuilderNodeDto> ParseAreas(string? widgetsJson, BuilderComponentLookup lookup, ICollection<string> errors)
    {
        using var document = TryParse(widgetsJson, "widgets (editable areas)", errors);
        if (document?.RootElement.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        return [.. GetObjects(document.RootElement, "editableAreas").Select(area => new BuilderNodeDto
        {
            Kind = BuilderNodeKinds.EDITABLE_AREA,
            Name = GetString(area, "identifier"),
            Children = [.. GetObjects(area, "sections").Select(section => ParseSection(section, lookup))],
        })];
    }

    private static BuilderNodeDto ParseSection(JsonElement section, BuilderComponentLookup lookup)
    {
        string? type = GetString(section, "type");
        var definition = lookup.FindSection(type);

        return new BuilderNodeDto
        {
            Kind = BuilderNodeKinds.SECTION,
            ComponentIdentifier = type,
            ComponentName = definition?.Name,
            IconClass = definition?.IconClass,
            IsRegistered = definition is not null,
            InstanceId = GetString(section, "identifier"),
            Properties = GetProperties(section, "properties"),
            Children = [.. GetObjects(section, "zones").Select(zone => new BuilderNodeDto
            {
                Kind = BuilderNodeKinds.ZONE,
                Name = GetString(zone, "name"),
                InstanceId = GetString(zone, "identifier"),
                Children = [.. GetObjects(zone, "widgets").Select(widget => ParseWidget(widget, lookup))],
            })],
        };
    }

    private static BuilderNodeDto ParseWidget(JsonElement widget, BuilderComponentLookup lookup)
    {
        string? type = GetString(widget, "type");
        var definition = lookup.FindWidget(type);

        return new BuilderNodeDto
        {
            Kind = BuilderNodeKinds.WIDGET,
            ComponentIdentifier = type,
            ComponentName = definition?.Name,
            IconClass = definition?.IconClass,
            IsRegistered = definition is not null,
            InstanceId = GetString(widget, "identifier"),
            ConditionType = GetString(widget, "conditionType"),
            Children = [.. GetObjects(widget, "variants").Select(variant => new BuilderNodeDto
            {
                Kind = BuilderNodeKinds.VARIANT,
                Name = GetString(variant, "name"),
                InstanceId = GetString(variant, "identifier"),
                Properties = GetProperties(variant, "properties"),
                ConditionParameters = GetProperties(variant, "conditionTypeParameters"),
            })],
        };
    }

    /// <summary>
    /// Compact form used to compare versions. Invalid JSON is compared as stored.
    /// </summary>
    internal static string? Normalize(string? json) => Format(json, compactOptions);

    private static string? Format(string? json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);

            return JsonSerializer.Serialize(document.RootElement, options);
        }
        catch (JsonException)
        {
            return json;
        }
    }

    private static JsonDocument? TryParse(string? json, string description, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            errors.Add($"The {description} JSON is not valid: {ex.Message}");
            return null;
        }
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static IEnumerable<JsonElement> GetObjects(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object)
            : [];

    private static List<BuilderPropertyDto> GetProperties(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Object
            ? [.. value.EnumerateObject().Select(p => new BuilderPropertyDto(p.Name, FormatValue(p.Value)))]
            : [];

    private static string FormatValue(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString() ?? string.Empty;
        }

        return value.ValueKind is JsonValueKind.Object or JsonValueKind.Array
            ? JsonSerializer.Serialize(value, compactOptions)
            : value.GetRawText();
    }
}
