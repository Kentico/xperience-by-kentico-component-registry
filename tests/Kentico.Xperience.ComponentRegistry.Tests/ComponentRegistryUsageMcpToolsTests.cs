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
    public void GetPageBuilderBatchUsage_RejectsUnsupportedType()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await tools.GetPageBuilderBatchUsage(["x"], "section"));
    }
}
