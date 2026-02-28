namespace Kentico.Xperience.ComponentRegistry.Tests;

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
}
