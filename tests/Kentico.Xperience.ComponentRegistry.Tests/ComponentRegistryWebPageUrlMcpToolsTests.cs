using Kentico.Xperience.ComponentRegistry.MCP.Internal;

using NSubstitute;

namespace Kentico.Xperience.ComponentRegistry.Tests;

public class ComponentRegistryWebPageUrlMcpToolsTests
{
    [Test]
    public async Task GetWebPageUrl_ReturnsPublishedResult_WhenPublishedRequested()
    {
        var generator = Substitute.For<IAgentPageUrlGenerator>();
        generator
            .Generate(123, "en-US", false, Arg.Any<CancellationToken>())
            .Returns("https://example.com/published/123");

        var tools = new ComponentRegistryWebPageUrlMcpTools(generator);

        var response = await tools.GetWebPageUrl(123, "en-US", true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.True);
            Assert.That(response.IsPublished, Is.True);
            Assert.That(response.Url, Is.EqualTo("https://example.com/published/123"));
        }

        await generator.Received(1)
            .Generate(123, "en-US", false, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetWebPageUrl_ReturnsPreviewResult_WhenPreviewRequested()
    {
        var generator = Substitute.For<IAgentPageUrlGenerator>();
        generator
            .Generate(123, "en-US", true, Arg.Any<CancellationToken>())
            .Returns("https://example.com/preview/123");

        var tools = new ComponentRegistryWebPageUrlMcpTools(generator);

        var response = await tools.GetWebPageUrl(123, "en-US", false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.True);
            Assert.That(response.IsPublished, Is.False);
            Assert.That(response.Url, Is.EqualTo("https://example.com/preview/123"));
        }

        await generator.Received(1)
            .Generate(123, "en-US", true, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetWebPageUrl_ReturnsFailure_WhenPreviewRequestedAndGeneratorReturnsEmpty()
    {
        var generator = Substitute.For<IAgentPageUrlGenerator>();
        generator
            .Generate(123, "en-US", true, Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        var tools = new ComponentRegistryWebPageUrlMcpTools(generator);

        var response = await tools.GetWebPageUrl(123, "en-US", false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.False);
            Assert.That(response.IsPublished, Is.False);
            Assert.That(response.Url, Is.Null);
            Assert.That(response.ErrorMessage, Is.EqualTo("Preview URL could not be generated for the requested page."));
        }
    }

    [Test]
    public async Task GetWebPageUrl_ReturnsSuccess_WhenPublishedRequestedAndGeneratorReturnsEmpty()
    {
        var generator = Substitute.For<IAgentPageUrlGenerator>();
        generator
            .Generate(123, "en-US", false, Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        var tools = new ComponentRegistryWebPageUrlMcpTools(generator);

        var response = await tools.GetWebPageUrl(123, "en-US", true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Success, Is.True);
            Assert.That(response.IsPublished, Is.True);
            Assert.That(response.Url, Is.EqualTo(string.Empty));
        }
    }
}
