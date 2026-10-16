namespace Kentico.Xperience.Labs.ComponentRegistry.Tests;

public class ComponentRegistryDefinitionMcpToolsTests
{
    [Test]
    public async Task ListComponentDefinitions_ReturnsExpectedPageItems()
    {
        var tools = new ComponentRegistryDefinitionMcpTools(
            new StubReadService(
                new PageBuilderRegistryReadModel(
                    Widgets: [new ComponentDto("w1", "Widget 1", null, null, null)],
                    Sections: [],
                    PageTemplates: [new PageTemplateDto("pt1", "Template 1", null, null, null, ["Acme.Page"])]),
                new EmailBuilderRegistryReadModel([], [], []),
                new FormBuilderRegistryReadModel([], [])));

        var response = await tools.ListComponentDefinitions("page", "all");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.Builder, Is.EqualTo("page"));
            Assert.That(response.Items, Has.Count.EqualTo(2));
            Assert.That(response.Items.Any(i => i.ComponentType == "widget" && i.Identifier == "w1"), Is.True);
            Assert.That(response.Items.Any(i => i.ComponentType == "page-template" && i.Identifier == "pt1"), Is.True);
        }
    }

    [Test]
    public async Task ListComponentDefinitions_AdminForm_ReturnsAdminComponentsOnly()
    {
        var tools = new ComponentRegistryDefinitionMcpTools(
            new StubReadService(
                new PageBuilderRegistryReadModel([], [], []),
                new EmailBuilderRegistryReadModel([], [], []),
                new FormBuilderRegistryReadModel([new FormComponentDto("builder.component", "Builder", null, null, null)], []),
                adminForm: new AdminFormComponentRegistryReadModel([new AdminFormComponentDto("Acme.Admin.Input", "Input", "Acme.Input", true)])));

        var response = await tools.ListComponentDefinitions("admin-form");

        Assert.That(response.Items, Has.Count.EqualTo(1));
        Assert.That(response.Items[0].Builder, Is.EqualTo("admin-form"));
        Assert.That(response.Items[0].Identifier, Is.EqualTo("Acme.Admin.Input"));
        Assert.That(response.Items[0].MarkedTypeName, Is.EqualTo("Acme.Input"));
    }

    [TestCase("page", "page.section")]
    [TestCase("email", "email.section")]
    [TestCase("form", "form.section")]
    public async Task ListComponentDefinitions_ReturnsSectionItems(string builder, string expectedIdentifier)
    {
        var tools = new ComponentRegistryDefinitionMcpTools(
            new StubReadService(
                new PageBuilderRegistryReadModel([], [new ComponentDto("page.section", "Page section", null, null, null)], []),
                new EmailBuilderRegistryReadModel([], [new EmailComponentDto("email.section", "Email section", null, null, null, null)], []),
                new FormBuilderRegistryReadModel([], [new FormSectionDto("form.section", "Form section", null, null, null)])));

        var response = await tools.ListComponentDefinitions(builder, "section");

        Assert.That(response.Items, Has.Count.EqualTo(1));
        Assert.That(response.Items[0].ComponentType, Is.EqualTo("section"));
        Assert.That(response.Items[0].Identifier, Is.EqualTo(expectedIdentifier));
    }
}
