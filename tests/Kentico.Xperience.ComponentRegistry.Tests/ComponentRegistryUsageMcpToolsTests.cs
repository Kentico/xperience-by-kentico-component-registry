namespace Kentico.Xperience.ComponentRegistry.Tests;

public class ComponentRegistryUsageMcpToolsTests
{
    [Test]
    public async Task GetComponentUsage_RoutesToExpectedUsageMethod()
    {
        var usage = new StubUsageService();
        var tools = new ComponentRegistryUsageMcpTools(usage);

        _ = await tools.GetComponentUsage("form", "section", "form.section");

        Assert.That(usage.LastCall, Is.EqualTo("form-section:form.section"));
    }

    [Test]
    public async Task GetComponentUsage_PageBuilderResponse_ContainsContentTypeDisplayName()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());

        var result = await tools.GetComponentUsage("page", "widget", "page.widget");

        Assert.That(result, Is.TypeOf<ComponentUsageMcpResponse>());
        var usage = (ComponentUsageMcpResponse)result;
        Assert.That(usage.Pages, Has.Count.EqualTo(1));
        Assert.That(usage.Pages[0].ContentTypeDisplayName, Is.EqualTo("Article"));
    }

    [Test]
    public void GetPageBuilderBatchUsage_RejectsUnsupportedType()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await tools.GetPageBuilderBatchUsage(["x"], "section"));
    }

    [Test]
    public async Task GetPageBuilderBatchUsage_MapsPageUsageMetadata()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());

        var result = await tools.GetPageBuilderBatchUsage(["widget.a"], "widget");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Pages[0].ContentTypeDisplayName, Is.EqualTo("Article"));
    }
}
