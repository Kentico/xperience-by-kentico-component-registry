using System.ComponentModel;

using Kentico.Xperience.ComponentRegistry.MCP.Internal;

using ModelContextProtocol.Server;

namespace Kentico.Xperience.ComponentRegistry.MCP;

/// <summary>
/// MCP tools exposing web page URL retrieval for published and unpublished variants.
/// </summary>
[McpServerToolType]
public class ComponentRegistryWebPageUrlMcpTools(IAgentPageUrlGenerator agentPageUrlGenerator)
{
    /// <summary>
    /// Gets the absolute URL for a web page by its ID and language.
    /// </summary>
    [McpServerTool(Name = "component_registry_get_web_page_url")]
    [Description("Get URL for a web page variant. Set isPublished=true for the live URL or isPublished=false for the preview URL.")]
    public async Task<WebPageUrlResponse> GetWebPageUrl(
        int webPageItemId,
        string languageName,
        bool isPublished,
        CancellationToken cancellationToken = default)
    {
        ComponentRegistryMcpToolsValidation.ValidateWebPageUrlRequest(webPageItemId, languageName);

        try
        {
            string webPageUrl = await agentPageUrlGenerator.Generate(
                webPageItemId,
                languageName,
                isLatest: !isPublished,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(webPageUrl))
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: isPublished,
                    Url: webPageUrl,
                    Success: true,
                    ErrorMessage: null);
            }

            if (isPublished)
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: true,
                    Url: webPageUrl,
                    Success: true,
                    ErrorMessage: null);
            }

            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: false,
                Url: null,
                Success: false,
                ErrorMessage: "Preview URL could not be generated for the requested page.");
        }
        catch (Exception ex)
        {
            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: isPublished,
                Url: null,
                Success: false,
                ErrorMessage: $"Failed to retrieve URL for web page {webPageItemId}: {ex.Message}");
        }
    }
}
