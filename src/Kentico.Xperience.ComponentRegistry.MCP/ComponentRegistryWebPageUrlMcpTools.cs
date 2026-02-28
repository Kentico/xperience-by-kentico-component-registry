using System.ComponentModel;

using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;

using Kentico.Xperience.ComponentRegistry;

using Kentico.Content.Web.Mvc.Internal;

using Microsoft.Extensions.Configuration;

using ModelContextProtocol.Server;

using WebPageItemInfo = CMS.Websites.Internal.WebPageItemInfo;

namespace Kentico.Xperience.ComponentRegistry.MCP;

/// <summary>
/// MCP tools exposing published and preview web page URL retrieval.
/// </summary>
[McpServerToolType]
public class ComponentRegistryWebPageUrlMcpTools(
    IWebPageUrlRetriever webPageUrlRetriever,
    IShareablePreviewLinkGenerator shareablePreviewLinkGenerator,
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<WebPageItemInfo> webPageItemProvider,
    IInfoProvider<UserInfo> userInfoProvider,
    IConfiguration configuration)
{
    private readonly ComponentRegistryMcpOptions options =
        configuration.GetSection(ComponentRegistryMcpOptions.SectionPath)
            .Get<ComponentRegistryMcpOptions>() ?? new ComponentRegistryMcpOptions();

    /// <summary>
    /// Gets the absolute published URL for a web page by its ID and language.
    /// </summary>
    [McpServerTool(Name = "component_registry_get_web_page_url")]
    [Description("Get absolute URL for a published web page. Returns the live URL that agents can use to visit and validate published rendering.")]
    public async Task<WebPageUrlResponse> GetWebPageUrl(
        int webPageItemId,
        string languageName,
        CancellationToken cancellationToken = default)
    {
        ComponentRegistryMcpToolsValidation.ValidateWebPageUrlRequest(webPageItemId, languageName);

        try
        {
            var webPageUrl = await webPageUrlRetriever.Retrieve(
                webPageItemId,
                languageName,
                forPreview: false,
                cancellationToken);

            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: true,
                UrlType: WebPageUrlType.Published,
                Url: webPageUrl.AbsoluteUrl,
                Success: true,
                ErrorMessage: null);
        }
        catch (Exception ex)
        {
            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: true,
                UrlType: null,
                Url: null,
                Success: false,
                ErrorMessage: $"Failed to retrieve URL for web page {webPageItemId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a shareable preview URL for unpublished page changes by web page ID and language.
    /// </summary>
    [McpServerTool(Name = "component_registry_get_web_page_preview_url")]
    [Description("Get shareable preview URL for an unpublished web page variant. Returns a URL that agents can use to visit and validate draft rendering, indicates whether the preview URL already existed or was newly generated, and returns an actionable error if the configured agent administration user is missing.")]
    public async Task<WebPageUrlResponse> GetWebPagePreviewUrl(
        int webPageItemId,
        string languageName,
        CancellationToken cancellationToken = default)
    {
        ComponentRegistryMcpToolsValidation.ValidateWebPageUrlRequest(webPageItemId, languageName);

        try
        {
            string configuredAgentUserName = options.AgentAdminUserName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(configuredAgentUserName))
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: false,
                    UrlType: null,
                    Url: null,
                    Success: false,
                    ErrorMessage: $"Preview URL requires a configured agent administration user. Set {ComponentRegistryMcpOptions.SectionPath}:AgentAdminUserName and create that administration user in Xperience.",
                    PreviewUrlState: null);
            }

            int itemWebsiteChannelId = await GetWebsiteChannelIdAsync(webPageItemId);

            int userID = await GetAgentUserIdAsync(configuredAgentUserName);
            if (userID <= 0)
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: false,
                    UrlType: null,
                    Url: null,
                    Success: false,
                    ErrorMessage: $"Preview URL requires administration user '{configuredAgentUserName}'. Create this user in Xperience administration (or update {ComponentRegistryMcpOptions.SectionPath}:AgentAdminUserName) to enable preview URL generation.",
                    PreviewUrlState: null);
            }

            var webPageManager = webPageManagerFactory.Create(itemWebsiteChannelId, userID);
            bool previewUrlAlreadyExisted = await EnsureShareablePreviewGuidAsync(
                webPageManager,
                webPageItemId,
                languageName,
                cancellationToken);

            string shareableUrl = await shareablePreviewLinkGenerator.Generate(webPageItemId, languageName, cancellationToken);
            if (!string.IsNullOrWhiteSpace(shareableUrl))
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: false,
                    UrlType: WebPageUrlType.ShareablePreview,
                    Url: shareableUrl,
                    Success: true,
                    ErrorMessage: null,
                    PreviewUrlState: previewUrlAlreadyExisted
                        ? PreviewUrlState.Existing
                        : PreviewUrlState.Generated);
            }

            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: false,
                UrlType: null,
                Url: null,
                Success: false,
                ErrorMessage: "Shareable preview URL could not be generated for the requested page.",
                PreviewUrlState: null);
        }
        catch (Exception ex)
        {
            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: false,
                UrlType: null,
                Url: null,
                Success: false,
                ErrorMessage: $"Failed to generate shareable preview URL for web page {webPageItemId}: {ex.Message}",
                PreviewUrlState: null);
        }
    }

    /// <summary>
    /// Removes shareable preview URL metadata for a web page language variant.
    /// </summary>
    [McpServerTool(Name = "component_registry_remove_web_page_preview_url")]
    [Description("Remove shareable preview URL metadata for an unpublished web page variant to clean up previously generated preview links.")]
    public async Task<WebPageUrlResponse> RemoveWebPagePreviewUrl(
        int webPageItemId,
        string languageName,
        CancellationToken cancellationToken = default)
    {
        ComponentRegistryMcpToolsValidation.ValidateWebPageUrlRequest(webPageItemId, languageName);

        try
        {
            string configuredAgentUserName = options.AgentAdminUserName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(configuredAgentUserName))
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: false,
                    UrlType: null,
                    Url: null,
                    Success: false,
                    ErrorMessage: $"Preview URL cleanup requires a configured agent administration user. Set {ComponentRegistryMcpOptions.SectionPath}:AgentAdminUserName and create that administration user in Xperience.",
                    PreviewUrlState: null);
            }

            int itemWebsiteChannelId = await GetWebsiteChannelIdAsync(webPageItemId);

            int userID = await GetAgentUserIdAsync(configuredAgentUserName);
            if (userID <= 0)
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: false,
                    UrlType: null,
                    Url: null,
                    Success: false,
                    ErrorMessage: $"Preview URL cleanup requires administration user '{configuredAgentUserName}'. Create this user in Xperience administration (or update {ComponentRegistryMcpOptions.SectionPath}:AgentAdminUserName) to enable preview URL cleanup.",
                    PreviewUrlState: null);
            }

            var webPageManager = webPageManagerFactory.Create(itemWebsiteChannelId, userID);
            var languageMetadata = await webPageManager.GetContentItemLanguageMetadata(
                webPageItemId,
                languageName);

            if (!languageMetadata.GetShareablePreviewGUID().HasValue)
            {
                return new WebPageUrlResponse(
                    WebPageItemId: webPageItemId,
                    LanguageName: languageName,
                    IsPublished: false,
                    UrlType: WebPageUrlType.ShareablePreview,
                    Url: null,
                    Success: true,
                    ErrorMessage: null,
                    PreviewUrlState: PreviewUrlState.NotFound);
            }

            languageMetadata.SetShareablePreviewGUID(null);
            await webPageManager.UpdateLanguageMetadata(languageMetadata, cancellationToken);

            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: false,
                UrlType: WebPageUrlType.ShareablePreview,
                Url: null,
                Success: true,
                ErrorMessage: null,
                PreviewUrlState: PreviewUrlState.Removed);
        }
        catch (Exception ex)
        {
            return new WebPageUrlResponse(
                WebPageItemId: webPageItemId,
                LanguageName: languageName,
                IsPublished: false,
                UrlType: null,
                Url: null,
                Success: false,
                ErrorMessage: $"Failed to remove shareable preview URL for web page {webPageItemId}: {ex.Message}",
                PreviewUrlState: null);
        }
    }

    protected virtual async Task<int> GetWebsiteChannelIdAsync(int webPageItemId) =>
        (await webPageItemProvider.GetAsync(webPageItemId)).WebPageItemWebsiteChannelID;

    protected virtual async Task<int> GetAgentUserIdAsync(string configuredAgentUserName) =>
        (await userInfoProvider.Get().WhereEquals(nameof(UserInfo.UserName), configuredAgentUserName).GetEnumerableTypedResultAsync())
            .Select(u => u.UserID)
            .FirstOrDefault();

    protected virtual async Task<bool> EnsureShareablePreviewGuidAsync(
        IWebPageManager webPageManager,
        int webPageItemId,
        string languageName,
        CancellationToken cancellationToken)
    {
        var languageMetadata = await webPageManager.GetContentItemLanguageMetadata(
            webPageItemId,
            languageName);

        bool previewUrlAlreadyExisted = languageMetadata.GetShareablePreviewGUID().HasValue;
        if (!previewUrlAlreadyExisted)
        {
            languageMetadata.SetShareablePreviewGUID(Guid.NewGuid());
            await webPageManager.UpdateLanguageMetadata(languageMetadata, cancellationToken);
        }

        return previewUrlAlreadyExisted;
    }
}
