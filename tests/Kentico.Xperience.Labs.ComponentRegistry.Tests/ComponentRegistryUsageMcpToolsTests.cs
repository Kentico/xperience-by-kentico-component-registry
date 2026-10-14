namespace Kentico.Xperience.Labs.ComponentRegistry.Tests;

public class ComponentRegistryUsageMcpToolsTests
{
    [Test]
    [TestCase("page", "section", "page.section", "page-section:page.section")]
    [TestCase("email", "section", "email.section", "email-section:email.section")]
    [TestCase("form", "section", "form.section", "form-section:form.section")]
    [TestCase("admin-form", "component", "Acme.Admin.Input", "admin-form-component:Acme.Admin.Input")]
    [TestCase("ui-form", "components", "Acme.Admin.Input", "admin-form-component:Acme.Admin.Input")]
    [TestCase("automation", "trigger", "a.trigger", "automation-trigger:a.trigger")]
    [TestCase("automation", "actions", "a.action", "automation-action:a.action")]
    [TestCase("automation", "condition", "a.condition", "automation-condition:a.condition")]
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
    public async Task GetComponentUsage_PageBuilderResponse_ContainsContentTypeDisplayName()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());

        object result = await tools.GetComponentUsage("page", "widget", "page.widget");

        Assert.That(result, Is.TypeOf<ComponentUsageMcpResponse>());
        var usage = (ComponentUsageMcpResponse)result;
        Assert.That(usage.Pages, Has.Count.EqualTo(1));
        Assert.That(usage.Pages[0].ContentTypeDisplayName, Is.EqualTo("Article"));
    }

    [Test]
    public async Task GetComponentUsage_PageSection_ReturnsMcpResponse()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());

        object result = await tools.GetComponentUsage("page", "section", "page.section");

        Assert.That(result, Is.TypeOf<ComponentUsageMcpResponse>());
    }

    [Test]
    public async Task GetPageBuilderBatchUsage_RejectsUnsupportedType()
    {
        var tools = new ComponentRegistryUsageMcpTools(new StubUsageService());
        await Assert.ThrowsAsync<ArgumentException>(async () =>
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
