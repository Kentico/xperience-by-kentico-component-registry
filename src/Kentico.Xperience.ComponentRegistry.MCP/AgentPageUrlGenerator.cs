using System.Collections.Specialized;

using CMS.DataEngine;
using CMS.Helpers.Internal;
using CMS.Websites;
using CMS.Websites.Internal;

namespace Kentico.Xperience.ComponentRegistry.MCP.Internal
{
    public interface IAgentPageUrlGenerator
    {
        public Task<string> Generate(int webPageItemId, string languageName, bool isLatest, CancellationToken cancellationToken = default);
    }

    internal sealed class AgentPageUrlGenerator(
        IWebPageUrlRetriever webPageUrlRetriever,
        IWebsiteChannelDomainProvider websiteChannelDomainProvider,
        IAbsoluteUrlBuilder absoluteUrlBuilder,
        IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
        TimeProvider timeProvider) : IAgentPageUrlGenerator
    {
        public async Task<string> Generate(int webPageItemId, string languageName, bool isLatest, CancellationToken cancellationToken = default)
        {
            var webPageItem = await webPageItemInfoProvider.GetAsync(webPageItemId, cancellationToken)
                ?? throw new WebPageNotFoundException($"Web page item with ID {webPageItemId} does not exist.");

            var virtualContextParams = VirtualContext.GetCrawlerModeParameters(
                languageName,
                webPageItemId,
                webPageItem.WebPageItemWebsiteChannelID,
                isLatest,
                GetExpirationTicks());

            string relativePath = await GetRelativePath(webPageItemId, languageName, isLatest, virtualContextParams, cancellationToken);

            string domain = await websiteChannelDomainProvider.GetDomain(webPageItem.WebPageItemWebsiteChannelID, cancellationToken);
            string absoluteUrl = absoluteUrlBuilder.BuildAbsoluteUrl(domain, relativePath);

            return absoluteUrl;
        }

        private long GetExpirationTicks() =>
            timeProvider.GetLocalNow()
                .Add(TimeSpan.FromMinutes(60.0))
                .ToUniversalTime()
                .Ticks;

        private async Task<string> GetRelativePath(int webPageItemId, string languageName, bool isLatest, NameValueCollection virtualContextParams, CancellationToken cancellationToken)
        {
            string virtualPath = (await webPageUrlRetriever.Retrieve(webPageItemId, languageName, isLatest, cancellationToken)).RelativePath;

            virtualPath = VirtualContext.AddPathHash(virtualPath).TrimStart('/');
            virtualPath = VirtualContext.GetVirtualContextPath(virtualPath, virtualContextParams);

            return virtualPath.TrimStart('~').ToLowerInvariant();
        }
    }
}
