namespace Kentico.Xperience.ComponentRegistry.MCP;

/// <summary>
/// Flattened component definition item returned by MCP list tools.
/// </summary>
public record ComponentDefinitionItem(
    string Builder,
    string ComponentType,
    string Identifier,
    string Name,
    string? Description,
    string? IconClass,
    string? MarkedTypeName,
    string? PropertiesTypeName,
    IReadOnlyList<string>? ContentTypeNames);

/// <summary>
/// Response payload for MCP definition list tool.
/// </summary>
public record ComponentDefinitionListResponse(
    string Builder,
    string ComponentType,
    IReadOnlyList<ComponentDefinitionItem> Items);

/// <summary>
/// Response payload for web page URL retrieval tool.
/// Contains absolute URL for a web page (published or shareable preview) or an error message if retrieval failed.
/// </summary>
public record WebPageUrlResponse(
    int WebPageItemId,
    string LanguageName,
    bool IsPublished,
    string? Url,
    bool Success,
    string? ErrorMessage);

internal static class ComponentRegistryMcpToolsValidation
{
    public static void ValidateWebPageUrlRequest(int webPageItemId, string languageName)
    {
        if (string.IsNullOrWhiteSpace(languageName))
        {
            throw new ArgumentException("Language name is required.", nameof(languageName));
        }

        if (webPageItemId <= 0)
        {
            throw new ArgumentException("Web page item ID must be greater than zero.", nameof(webPageItemId));
        }
    }

    public static string NormalizeBuilder(string builder) => builder?.Trim().ToLowerInvariant() switch
    {
        "page" or "pagebuilder" or "page-builder" => "page",
        "email" or "emailbuilder" or "email-builder" => "email",
        "form" or "formbuilder" or "form-builder" => "form",
        _ => builder?.Trim().ToLowerInvariant() ?? string.Empty
    };

    public static string NormalizeComponentType(string componentType) => componentType?.Trim().ToLowerInvariant() switch
    {
        "all" => "all",
        "widget" or "widgets" => "widget",
        "section" or "sections" => "section",
        "template" or "templates" => "template",
        "pagetemplate" or "page-template" or "page_templates" => "page-template",
        "component" or "components" => "component",
        _ => componentType?.Trim().ToLowerInvariant() ?? string.Empty
    };
}
