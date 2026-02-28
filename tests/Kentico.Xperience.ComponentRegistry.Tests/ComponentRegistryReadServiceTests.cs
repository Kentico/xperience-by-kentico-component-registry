namespace Kentico.Xperience.ComponentRegistry.Tests;

public class ComponentRegistryReadServiceTests
{
    [Test]
    public async Task GetPageBuilderRegistryAsync_MapsAndLocalizesDefinitions()
    {
        var widgetStore = new InMemoryStore<PageBuilderWidgetDefinition>([
            new PageBuilderWidgetDefinition("widget.one", "Widget One", typeof(string), "Widget desc", "icon-widget")
        ]);
        var sectionStore = new InMemoryStore<PageBuilderSectionDefinition>([
            new PageBuilderSectionDefinition("section.one", "Section One", typeof(int), "Section desc", "icon-section")
        ]);
        var templateStore = new InMemoryStore<PageBuilderPageTemplateDefinition>([
            new PageBuilderPageTemplateDefinition("template.one", "Template One", typeof(Uri), "Template desc", "icon-template", ["Acme.Page"])
        ]);

        var service = CreateReadService(
            pageWidgetStore: widgetStore,
            pageSectionStore: sectionStore,
            pageTemplateStore: templateStore,
            localizationService: new PrefixLocalizationService("loc:"));

        var model = await service.GetPageBuilderRegistryAsync();

        Assert.That(model.Widgets, Has.Count.EqualTo(1));
        Assert.That(model.Sections, Has.Count.EqualTo(1));
        Assert.That(model.PageTemplates, Has.Count.EqualTo(1));
        Assert.That(model.Widgets[0].Name, Is.EqualTo("loc:Widget One"));
        Assert.That(model.Widgets[0].Description, Is.EqualTo("loc:Widget desc"));
        Assert.That(model.PageTemplates[0].ContentTypeNames, Is.EqualTo(new[] { "Acme.Page" }));
    }

    [Test]
    public async Task GetEmailAndFormRegistryAsync_MapsExpectedFields()
    {
        var emailWidgetStore = new InMemoryStore<EmailBuilderWidgetDefinition>([
            new EmailBuilderWidgetDefinition("email.widget", "Email Widget", typeof(string), "Email widget desc", "icon-email-widget", typeof(Guid))
        ]);
        var emailSectionStore = new InMemoryStore<EmailBuilderSectionDefinition>([
            new EmailBuilderSectionDefinition("email.section", "Email Section", typeof(decimal), "Email section desc", "icon-email-section")
        ]);
        var emailTemplateStore = new InMemoryStore<EmailBuilderTemplateDefinition>([
            new EmailBuilderTemplateDefinition("email.template", "Email Template", typeof(DateTime), "Email template desc", "icon-email-template", ["Acme.Email"])
        ]);

        var formComponentStore = new InMemoryStore<FormBuilderComponentDefinition>([
            new FormBuilderComponentDefinition("form.component", "Form Component", typeof(string), "Form comp desc", "icon-form-comp")
        ]);
        var formSectionStore = new InMemoryStore<FormBuilderSectionDefinition>([
            new FormBuilderSectionDefinition("form.section", "Form Section", typeof(int), "Form section desc", "icon-form-section")
        ]);

        var service = CreateReadService(
            emailWidgetStore: emailWidgetStore,
            emailSectionStore: emailSectionStore,
            emailTemplateStore: emailTemplateStore,
            formComponentStore: formComponentStore,
            formSectionStore: formSectionStore,
            localizationService: new PrefixLocalizationService("loc:"));

        var emailModel = await service.GetEmailBuilderRegistryAsync();
        var formModel = await service.GetFormBuilderRegistryAsync();

        Assert.That(emailModel.Widgets[0].PropertiesTypeName, Is.EqualTo(typeof(Guid).FullName));
        Assert.That(emailModel.EmailTemplates[0].ContentTypeNames, Is.EqualTo(new[] { "Acme.Email" }));
        Assert.That(formModel.FormComponents[0].Name, Is.EqualTo("loc:Form Component"));
        Assert.That(formModel.FormSections[0].Name, Is.EqualTo("loc:Form Section"));
    }

    private static ComponentRegistryReadService CreateReadService(
        IComponentDefinitionStore<PageBuilderWidgetDefinition>? pageWidgetStore = null,
        IComponentDefinitionStore<PageBuilderSectionDefinition>? pageSectionStore = null,
        IComponentDefinitionStore<PageBuilderPageTemplateDefinition>? pageTemplateStore = null,
        IComponentDefinitionStore<EmailBuilderWidgetDefinition>? emailWidgetStore = null,
        IComponentDefinitionStore<EmailBuilderSectionDefinition>? emailSectionStore = null,
        IComponentDefinitionStore<EmailBuilderTemplateDefinition>? emailTemplateStore = null,
        IComponentDefinitionStore<FormBuilderComponentDefinition>? formComponentStore = null,
        IComponentDefinitionStore<FormBuilderSectionDefinition>? formSectionStore = null,
        IComponentRegistryLocalizationService? localizationService = null)
        => new(
            pageWidgetStore ?? new InMemoryStore<PageBuilderWidgetDefinition>([]),
            pageSectionStore ?? new InMemoryStore<PageBuilderSectionDefinition>([]),
            pageTemplateStore ?? new InMemoryStore<PageBuilderPageTemplateDefinition>([]),
            emailWidgetStore ?? new InMemoryStore<EmailBuilderWidgetDefinition>([]),
            emailSectionStore ?? new InMemoryStore<EmailBuilderSectionDefinition>([]),
            emailTemplateStore ?? new InMemoryStore<EmailBuilderTemplateDefinition>([]),
            formComponentStore ?? new InMemoryStore<FormBuilderComponentDefinition>([]),
            formSectionStore ?? new InMemoryStore<FormBuilderSectionDefinition>([]),
            localizationService ?? new PrefixLocalizationService(string.Empty));
}

internal sealed class InMemoryStore<TDefinition>(IEnumerable<TDefinition> definitions) : IComponentDefinitionStore<TDefinition>
{
    private readonly List<TDefinition> items = definitions.ToList();

    public TDefinition? Get(string identifier) => items.Find(i =>
        i is IComponentDefinition definition &&
        definition.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase));

    public void Add(TDefinition registeredDefinition) => items.Add(registeredDefinition);

    public IEnumerable<TDefinition> GetAll() => items;
}

internal sealed class PrefixLocalizationService(string prefix) : IComponentRegistryLocalizationService
{
    public string Localize(string value) => $"{prefix}{value}";
}
