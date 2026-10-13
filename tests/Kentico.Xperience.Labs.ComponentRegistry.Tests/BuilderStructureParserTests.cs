namespace Kentico.Xperience.Labs.ComponentRegistry.Tests;

public class BuilderStructureParserTests
{
    private const string TEMPLATE_JSON = """
        {
          "identifier": "DancingGoat.LandingPageSingleColumn",
          "properties": {
            "showLogo": true,
            "headerColorCssClass": "first-color",
            "heroImage": [
              {
                "identifier": "8391b316-9628-4790-8034-42e672acc963"
              }
            ]
          },
          "fieldIdentifiers": {
            "showLogo": "d537d190-9d65-4811-8af1-10d0f4e1a240"
          }
        }
        """;

    private const string WIDGETS_JSON = """
        {
          "editableAreas": [
            {
              "identifier": "main",
              "sections": [
                {
                  "identifier": "eddd61ce-8228-4921-ae17-cc6e51c86ced",
                  "type": "DancingGoat.SingleColumnSection",
                  "properties": {
                    "theme": null
                  },
                  "zones": [
                    {
                      "identifier": "6c47b6e8-9948-48cd-ae94-f4ab0e5b17eb",
                      "widgets": [
                        {
                          "identifier": "cfca024b-5507-4171-9fe9-07f610589f1f",
                          "type": "DancingGoat.General.TestimonialWidget",
                          "conditionType": "DancingGoat.Personalization.IsInContactGroup",
                          "variants": [
                            {
                              "identifier": "fe13d34a-fe86-461f-93cc-83fee00be441",
                              "properties": {
                                "colorCssClass": "first-color"
                              },
                              "fieldIdentifiers": {
                                "colorCssClass": "e8fd336e-70d8-42c5-ad59-a064e805176a"
                              }
                            },
                            {
                              "identifier": "1cbb75c1-9eb0-47aa-80ef-eba6a2b21009",
                              "name": "Business customers",
                              "properties": {
                                "colorCssClass": "second-color"
                              },
                              "conditionTypeParameters": {
                                "selectedContactGroups": [
                                  {
                                    "objectGuid": "f3a9e2b1-4c8d-4e5f-9a10-2b3c4d5e6f70",
                                    "objectCodeName": "DancingGoat.BusinessCustomers"
                                  }
                                ],
                                "variantName": null
                              }
                            }
                          ]
                        },
                        {
                          "identifier": "ce82c2fb-c8e7-400d-a337-7b701f303a04",
                          "type": "Legacy.RemovedWidget",
                          "variants": [
                            {
                              "identifier": "c966eaeb-cc0e-44d8-aba0-2e751cfc0e25",
                              "properties": {
                                "content": "<h1 class=\"display\">Café</h1>"
                              }
                            }
                          ]
                        }
                      ]
                    }
                  ],
                  "fieldIdentifiers": {
                    "theme": "71c9bd25-04ef-42b0-9f2c-f88909c3d87a"
                  }
                }
              ]
            }
          ]
        }
        """;

    private const string EMAIL_TEMPLATE_JSON = """
        {
          "identifier": "DancingGoat.DancingGoatEmailBuilderTemplate",
          "properties": null,
          "fieldIdentifiers": {}
        }
        """;

    private const string EMAIL_WIDGETS_JSON = """
        {
          "editableAreas": [
            {
              "identifier": "MainContent",
              "sections": [
                {
                  "identifier": "a5b7fd8a-f25c-4dac-857d-79bd49517ab7",
                  "type": "DancingGoat.DancingGoatFullWidthEmailSection",
                  "properties": null,
                  "zones": [
                    {
                      "identifier": "5a85b8a2-19f9-4444-b847-d3abe8b36b0e",
                      "name": "Widget_Zone_0",
                      "widgets": [
                        {
                          "identifier": "d12683d8-2032-4271-bd40-80a4609c1099",
                          "type": "DancingGoat.DancingGoatButtonWidget",
                          "variants": [
                            {
                              "identifier": "2a9fcd76-a15f-41fd-bcda-c4a655718590",
                              "properties": {
                                "text": "Articles",
                                "buttonType": "Link"
                              },
                              "fieldIdentifiers": {
                                "text": "35f66220-f0c1-4582-9cc8-38f8d24bfe80",
                                "buttonType": "62b1ca91-39fb-4564-82e7-fad21a98b821"
                              }
                            }
                          ]
                        }
                      ]
                    }
                  ],
                  "fieldIdentifiers": {}
                }
              ]
            }
          ]
        }
        """;

    private static readonly BuilderComponentLookup registry = BuilderComponentLookup.From(new PageBuilderRegistryReadModel(
        Widgets: [new ComponentDto("DancingGoat.General.TestimonialWidget", "Testimonial", null, "icon-l-text", null)],
        Sections: [new ComponentDto("DancingGoat.SingleColumnSection", "Single column section", null, "icon-square", null)],
        PageTemplates: [new PageTemplateDto("DancingGoat.LandingPageSingleColumn", "Landing page", null, "icon-l-header-text", null, [])]));

    private static readonly BuilderComponentLookup emailRegistry = BuilderComponentLookup.From(new EmailBuilderRegistryReadModel(
        Widgets: [new EmailComponentDto("DancingGoat.DancingGoatButtonWidget", "Button", null, "icon-rectangle-a", null, null)],
        Sections: [new EmailComponentDto("DancingGoat.DancingGoatFullWidthEmailSection", "Full width", null, "icon-square", null, null)],
        EmailTemplates: [new EmailTemplateDto("DancingGoat.DancingGoatEmailBuilderTemplate", "Dancing Goat email", null, "icon-l-header-text", null, [])]));

    [Test]
    public void Parse_TemplateAndWidgets_BuildsNestedTreeWithRegistryData()
    {
        var errors = new List<string>();

        var template = BuilderStructureParser.Parse(WIDGETS_JSON, TEMPLATE_JSON, registry, errors);

        Assert.That(errors, Is.Empty);
        Assert.That(template, Is.Not.Null);
        Assert.That((template!.Kind, template.ComponentIdentifier, template.ComponentName, template.IconClass, template.IsRegistered),
            Is.EqualTo((BuilderNodeKinds.TEMPLATE, "DancingGoat.LandingPageSingleColumn", "Landing page", "icon-l-header-text", (bool?)true)));
        Assert.That(template.Properties.Select(p => (p.Name, p.Value)), Is.EqualTo(new[]
        {
            ("showLogo", "true"),
            ("headerColorCssClass", "first-color"),
            ("heroImage", """[{"identifier":"8391b316-9628-4790-8034-42e672acc963"}]"""),
        }));

        var area = template.Children.Single();
        Assert.That((area.Kind, area.Name), Is.EqualTo((BuilderNodeKinds.EDITABLE_AREA, "main")));

        var section = area.Children.Single();
        Assert.That((section.Kind, section.ComponentName, section.InstanceId),
            Is.EqualTo((BuilderNodeKinds.SECTION, "Single column section", "eddd61ce-8228-4921-ae17-cc6e51c86ced")));
        Assert.That(section.Properties.Select(p => (p.Name, p.Value)), Is.EqualTo(new[] { ("theme", "null") }));

        var zone = section.Children.Single();
        Assert.That((zone.Kind, zone.Name, zone.InstanceId), Is.EqualTo((BuilderNodeKinds.ZONE, (string?)null, "6c47b6e8-9948-48cd-ae94-f4ab0e5b17eb")));
        Assert.That(zone.Children.Select(w => w.Kind), Is.All.EqualTo(BuilderNodeKinds.WIDGET));
    }

    [Test]
    public void Parse_PersonalizedWidget_ReadsConditionTypeAndVariants()
    {
        var widget = GetWidgets(BuilderStructureParser.Parse(WIDGETS_JSON, null, registry, []))[0];

        Assert.That((widget.ComponentName, widget.IsRegistered, widget.ConditionType),
            Is.EqualTo(("Testimonial", (bool?)true, "DancingGoat.Personalization.IsInContactGroup")));
        Assert.That(widget.Children.Select(v => (v.Kind, v.Name)), Is.EqualTo(new[]
        {
            (BuilderNodeKinds.VARIANT, null),
            (BuilderNodeKinds.VARIANT, "Business customers"),
        }));
        Assert.That(widget.Children[0].ConditionParameters, Is.Empty);
        Assert.That(widget.Children[1].ConditionParameters.Select(p => (p.Name, p.Value)), Is.EqualTo(new[]
        {
            ("selectedContactGroups", """[{"objectGuid":"f3a9e2b1-4c8d-4e5f-9a10-2b3c4d5e6f70","objectCodeName":"DancingGoat.BusinessCustomers"}]"""),
            ("variantName", "null"),
        }));
    }

    [Test]
    public void Parse_UnregisteredWidget_IsFlaggedAndKeepsReadableHtml()
    {
        var widget = GetWidgets(BuilderStructureParser.Parse(WIDGETS_JSON, null, registry, []))[1];

        Assert.That((widget.ComponentIdentifier, widget.ComponentName, widget.IsRegistered),
            Is.EqualTo(("Legacy.RemovedWidget", (string?)null, (bool?)false)));
        Assert.That(widget.Children.Single().Properties.Single().Value, Is.EqualTo("<h1 class=\"display\">Café</h1>"));
    }

    [Test]
    public void Parse_WidgetsWithoutTemplate_ReturnsRoot()
    {
        var root = BuilderStructureParser.Parse(WIDGETS_JSON, string.Empty, registry, []);

        Assert.That(root?.Kind, Is.EqualTo(BuilderNodeKinds.ROOT));
        Assert.That(root!.Children.Single().Name, Is.EqualTo("main"));
    }

    [TestCase(null, null)]
    [TestCase("""{ "editableAreas": [] }""", "")]
    public void Parse_NoBuilderData_ReturnsNull(string? widgetsJson, string? templateJson) =>
        Assert.That(BuilderStructureParser.Parse(widgetsJson, templateJson, registry, []), Is.Null);

    [Test]
    public void Parse_InvalidWidgetsJson_ReportsErrorAndKeepsTemplate()
    {
        var errors = new List<string>();

        var root = BuilderStructureParser.Parse("{ \"editableAreas\": [", TEMPLATE_JSON, registry, errors);

        Assert.That(root?.Kind, Is.EqualTo(BuilderNodeKinds.TEMPLATE));
        Assert.That(root!.Children, Is.Empty);
        Assert.That(errors, Has.Count.EqualTo(1).And.Some.StartsWith("The widgets (editable areas) JSON is not valid"));
    }

    [Test]
    public void Parse_EmailBuilderJson_ResolvesEmailBuilderComponents()
    {
        var errors = new List<string>();

        var template = BuilderStructureParser.Parse(EMAIL_WIDGETS_JSON, EMAIL_TEMPLATE_JSON, emailRegistry, errors);

        Assert.That(errors, Is.Empty);
        Assert.That((template?.Kind, template?.ComponentName, template?.IsRegistered),
            Is.EqualTo((BuilderNodeKinds.TEMPLATE, "Dancing Goat email", (bool?)true)));
        Assert.That(template!.Properties, Is.Empty);

        var area = template.Children.Single();
        var section = area.Children.Single();
        Assert.That((area.Name, section.ComponentName, section.Properties.Count), Is.EqualTo(("MainContent", "Full width", 0)));

        var zone = section.Children.Single();
        var widget = zone.Children.Single();
        Assert.That((zone.Name, widget.ComponentName, widget.IsRegistered, widget.ConditionType),
            Is.EqualTo(("Widget_Zone_0", "Button", (bool?)true, (string?)null)));
        Assert.That(widget.Children.Single().Properties.Select(p => (p.Name, p.Value)), Is.EqualTo(new[]
        {
            ("text", "Articles"),
            ("buttonType", "Link"),
        }));
    }

    [Test]
    public void Parse_EmailBuilderJsonWithPageBuilderRegistry_FlagsComponentsAsNotRegistered()
    {
        var template = BuilderStructureParser.Parse(EMAIL_WIDGETS_JSON, EMAIL_TEMPLATE_JSON, registry, []);
        var section = template!.Children.Single().Children.Single();

        Assert.That(new[] { template.IsRegistered, section.IsRegistered, section.Children.Single().Children.Single().IsRegistered },
            Is.All.False);
    }

    [Test]
    public void Create_ComparesLatestWithPublishedIgnoringFormatting()
    {
        var latest = new BuilderDataVersion(1, WIDGETS_JSON, TEMPLATE_JSON);
        var reformatted = new BuilderDataVersion(2, BuilderStructureParser.Normalize(WIDGETS_JSON), TEMPLATE_JSON);
        var changed = new BuilderDataVersion(2, WIDGETS_JSON.Replace("second-color", "third-color"), TEMPLATE_JSON);

        var unchanged = BuilderStructureParser.Create("en", "English", latest, reformatted, registry);

        Assert.Multiple(() =>
        {
            Assert.That(unchanged.VersionStatus, Is.EqualTo("Draft"));
            Assert.That(unchanged.DiffersFromPublished, Is.False);
            Assert.That(BuilderStructureParser.Create("en", "English", latest, changed, registry).DiffersFromPublished, Is.True);
            Assert.That(BuilderStructureParser.Create("en", "English", latest, null, registry).DiffersFromPublished, Is.Null);
        });
    }

    private static IReadOnlyList<BuilderNodeDto> GetWidgets(BuilderNodeDto? root) =>
        root!.Children.Single().Children.Single().Children.Single().Children;
}
