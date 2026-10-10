namespace Kentico.Xperience.ComponentRegistry.Tests;

public class ComponentRegistryUsageMcpToolsTests
{
    [Test]
    [TestCase("page", "section", "page.section", "page-section:page.section")]
    [TestCase("email", "section", "email.section", "email-section:email.section")]
    [TestCase("form", "section", "form.section", "form-section:form.section")]
    public async Task GetComponentUsage_RoutesSectionQueriesToExpectedUsageMethod(
        string builder,
        string sectionType,
        string identifier,
        string expectedCall)
    {
        var usage = new StubUsageService();
        var tools = new ComponentRegistryUsageMcpTools(usage);

        _ = await tools.GetComponentUsage(builder, sectionType, identifier);

        Assert.That(usage.LastCall, Is.EqualTo(expectedCall));
    }

    [Test]
    public void GetPageBuilderBatchUsage_RejectsUnsupportedType()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await tools.GetPageBuilderBatchUsage(["x"], "section"));
    }
}
