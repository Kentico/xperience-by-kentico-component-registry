using System.Reflection;

using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;

using Kentico.Content.Web.Mvc.Internal;

using Microsoft.Extensions.Configuration;

using NSubstitute;

namespace Kentico.Xperience.ComponentRegistry.Tests;

public class ComponentRegistryWebPageUrlMcpToolsTests
{
    [Test]
    public async Task GetWebPageUrl_ReturnsPublishedUrl()
    {
        var tools = new ComponentRegistryWebPageUrlMcpTools(
            new StubWebPageUrlRetriever(),
            null!,
            null!,
            null!,
            null!,
            CreateConfiguration());

        var response = await tools.GetWebPageUrl(123, "en-US");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.True);
            Assert.That(response.IsPublished, Is.True);
            Assert.That(response.UrlType, Is.EqualTo(WebPageUrlType.Published));
            Assert.That(response.Url, Is.EqualTo("https://example.com/en-US/page-123"));
        }
    }

    [Test]
    public async Task GetWebPagePreviewUrl_WhenAgentUserNotConfigured_ReturnsActionableError()
    {
        var tools = new ComponentRegistryWebPageUrlMcpTools(
            new StubWebPageUrlRetriever(),
            null!,
            null!,
            null!,
            null!,
            CreateConfiguration(" "));

        var response = await tools.GetWebPagePreviewUrl(123, "en-US");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.False);
            Assert.That(response.ErrorMessage, Does.Contain("AgentAdminUserName"));
            Assert.That(response.PreviewUrlState, Is.Null);
        }
    }

    [Test]
    public async Task RemoveWebPagePreviewUrl_WhenAgentUserNotConfigured_ReturnsActionableError()
    {
        var tools = new ComponentRegistryWebPageUrlMcpTools(
            new StubWebPageUrlRetriever(),
            null!,
            null!,
            null!,
            null!,
            CreateConfiguration(" "));

        var response = await tools.RemoveWebPagePreviewUrl(123, "en-US");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.False);
            Assert.That(response.ErrorMessage, Does.Contain("AgentAdminUserName"));
            Assert.That(response.PreviewUrlState, Is.Null);
        }
    }

    [Test]
    public async Task GetWebPagePreviewUrl_WhenPreviewUrlExists_ReturnsExistingState()
    {
        var webPageUrlRetriever = new StubWebPageUrlRetriever();
        var previewLinkGenerator = Substitute.For<IShareablePreviewLinkGenerator>();
        previewLinkGenerator
            .Generate(123, "en-US", Arg.Any<CancellationToken>())
            .Returns("https://example.com/preview/abc123");

        var webPageManagerFactory = Substitute.For<IWebPageManagerFactory>();
        var webPageManager = Substitute.For<IWebPageManager>();
        webPageManagerFactory.Create(7, 99).Returns(webPageManager);

        var metadata = TestableComponentRegistryWebPageUrlMcpTools.CreateLanguageMetadata();
        TestableComponentRegistryWebPageUrlMcpTools.SetShareablePreviewGuid(metadata, Guid.NewGuid());
        webPageManager.GetContentItemLanguageMetadata(123, "en-US").Returns(metadata);

        var webPageItemProvider = Substitute.For<IInfoProvider<WebPageItemInfo>>();
        var userInfoProvider = Substitute.For<IInfoProvider<UserInfo>>();

        var tools = new TestableComponentRegistryWebPageUrlMcpTools(
            webPageUrlRetriever,
            previewLinkGenerator,
            webPageManagerFactory,
            webPageItemProvider,
            userInfoProvider,
            CreateConfiguration("mcpAgent"),
            websiteChannelId: 7,
            agentUserId: 99);

        var response = await tools.GetWebPagePreviewUrl(123, "en-US");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.True);
            Assert.That(response.IsPublished, Is.False);
            Assert.That(response.UrlType, Is.EqualTo(WebPageUrlType.ShareablePreview));
            Assert.That(response.Url, Is.EqualTo("https://example.com/preview/abc123"));
            Assert.That(response.PreviewUrlState, Is.EqualTo(PreviewUrlState.Existing));
        }

        await webPageManager.DidNotReceive()
            .UpdateLanguageMetadata(Arg.Any<ContentItemLanguageMetadata>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetWebPagePreviewUrl_WhenPreviewUrlDoesNotExist_ReturnsGeneratedState()
    {
        var webPageUrlRetriever = new StubWebPageUrlRetriever();
        var previewLinkGenerator = Substitute.For<IShareablePreviewLinkGenerator>();
        previewLinkGenerator
            .Generate(123, "en-US", Arg.Any<CancellationToken>())
            .Returns("https://example.com/preview/generated123");

        var webPageManagerFactory = Substitute.For<IWebPageManagerFactory>();
        var webPageManager = Substitute.For<IWebPageManager>();
        webPageManagerFactory.Create(7, 99).Returns(webPageManager);

        var metadata = TestableComponentRegistryWebPageUrlMcpTools.CreateLanguageMetadata();
        TestableComponentRegistryWebPageUrlMcpTools.SetShareablePreviewGuid(metadata, null);
        webPageManager.GetContentItemLanguageMetadata(123, "en-US").Returns(metadata);

        var webPageItemProvider = Substitute.For<IInfoProvider<WebPageItemInfo>>();
        var userInfoProvider = Substitute.For<IInfoProvider<UserInfo>>();

        var tools = new TestableComponentRegistryWebPageUrlMcpTools(
            webPageUrlRetriever,
            previewLinkGenerator,
            webPageManagerFactory,
            webPageItemProvider,
            userInfoProvider,
            CreateConfiguration("mcpAgent"),
            websiteChannelId: 7,
            agentUserId: 99);

        var response = await tools.GetWebPagePreviewUrl(123, "en-US");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.True);
            Assert.That(response.IsPublished, Is.False);
            Assert.That(response.UrlType, Is.EqualTo(WebPageUrlType.ShareablePreview));
            Assert.That(response.Url, Is.EqualTo("https://example.com/preview/generated123"));
            Assert.That(response.PreviewUrlState, Is.EqualTo(PreviewUrlState.Generated));
            Assert.That(metadata.GetShareablePreviewGUID().HasValue, Is.True);
        }

        await webPageManager.Received(1)
            .UpdateLanguageMetadata(Arg.Any<ContentItemLanguageMetadata>(), Arg.Any<CancellationToken>());
    }

    private static IConfiguration CreateConfiguration(string? agentAdminUserName = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ComponentRegistryMcpOptions.SectionPath}:AgentAdminUserName"] =
                    agentAdminUserName ?? "mcpAgent"
            })
            .Build();
}

internal sealed class TestableComponentRegistryWebPageUrlMcpTools(
    IWebPageUrlRetriever webPageUrlRetriever,
    IShareablePreviewLinkGenerator shareablePreviewLinkGenerator,
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<WebPageItemInfo> webPageItemProvider,
    IInfoProvider<UserInfo> userInfoProvider,
    IConfiguration configuration,
    int websiteChannelId,
    int agentUserId)
    : ComponentRegistryWebPageUrlMcpTools(
        webPageUrlRetriever,
        shareablePreviewLinkGenerator,
        webPageManagerFactory,
        webPageItemProvider,
        userInfoProvider,
        configuration)
{
    protected override Task<int> GetWebsiteChannelIdAsync(int webPageItemId) => Task.FromResult(websiteChannelId);

    protected override Task<int> GetAgentUserIdAsync(string configuredAgentUserName) => Task.FromResult(agentUserId);

    internal static ContentItemLanguageMetadata CreateLanguageMetadata() =>
        new(
            contentItemId: 123,
            languageName: "en-US",
            displayName: "Sample",
            latestVersionStatus: VersionStatus.InitialDraft,
            createdWhen: DateTime.UtcNow,
            createdBy: 1,
            modifiedWhen: DateTime.UtcNow,
            modifiedBy: 1,
            hasImageAsset: false);

    internal static void SetShareablePreviewGuid(ContentItemLanguageMetadata metadata, Guid? value)
    {
        var property = typeof(ContentItemLanguageMetadata)
            .GetProperty("ShareablePreviewGUID", BindingFlags.Instance | BindingFlags.NonPublic);

        property?.SetValue(metadata, value);
    }
}

internal sealed class StubWebPageUrlRetriever : IWebPageUrlRetriever
{
    public Task<WebPageUrl> Retrieve(IWebPageFieldsSource webPageFieldsSource, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<WebPageUrl> Retrieve(IWebPageFieldsSource webPageFieldsSource, string languageName, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<WebPageUrl> Retrieve(string webPageUrlPath, string webPageTreePath, int websiteChannelId, string languageName, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<WebPageUrl> Retrieve(string webPageTreePath, string websiteChannelName, string languageName, bool forPreview = false, CancellationToken cancellationToken = default) =>
        Task.FromResult(new WebPageUrl($"/{languageName}/page", null));

    public Task<WebPageUrl> Retrieve(int webPageItemId, string languageName, bool forPreview = false, CancellationToken cancellationToken = default) =>
        Task.FromResult(new WebPageUrl($"/{languageName}/page-{webPageItemId}", $"https://example.com/{languageName}/page-{webPageItemId}"));

    public Task<WebPageUrl> Retrieve(Guid webPageItemGuid, string languageName, bool forPreview = false, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IDictionary<Guid, WebPageUrl>> Retrieve(IReadOnlyCollection<Guid> webPageItemGuids, string websiteChannelName, string languageName, bool forPreview = false, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
