using System.Xml;

namespace Kentico.Xperience.Labs.ComponentRegistry.Tests;

public class ClassFormDefinitionParserTests
{
    private const string DEFINITION = """
        <form>
          <field column="ArticleTitle" guid="11111111-1111-1111-1111-111111111111" columntype="text">
            <properties><fieldcaption>Title</fieldcaption></properties>
            <settings><controlname>Kentico.Administration.TextInput</controlname></settings>
          </field>
          <field column="ArticleBody" guid="22222222-2222-2222-2222-222222222222" columntype="longtext">
            <properties><fieldcaption>{$ body $}</fieldcaption></properties>
            <settings><controlname> kentico.administration.textinput </controlname></settings>
          </field>
          <field column="ArticleSummary" guid="33333333-3333-3333-3333-333333333333" columntype="text">
            <settings><controlname>Kentico.Administration.TextArea</controlname></settings>
          </field>
          <field column="FormBuilderField" guid="44444444-4444-4444-4444-444444444444" columntype="text">
            <settings><componentidentifier>Kentico.Administration.TextInput</componentidentifier></settings>
          </field>
          <category name="Kentico.Administration.TextInput" />
        </form>
        """;

    [Test]
    public void GetFieldsUsingControl_MatchesControlNameOnly_CaseInsensitiveAndTrimmed()
    {
        var fields = ClassFormDefinitionParser.GetFieldsUsingControl(DEFINITION, "Kentico.Administration.TextInput");

        Assert.That(fields.Select(f => (f.FieldName, f.FieldGuid, f.FieldCaption)), Is.EqualTo(new[]
        {
            ("ArticleTitle", "11111111-1111-1111-1111-111111111111", "Title"),
            ("ArticleBody", "22222222-2222-2222-2222-222222222222", "{$ body $}"),
        }));
    }

    [Test]
    public void GetFieldsUsingControl_NoMatch_ReturnsEmpty() => Assert.That(ClassFormDefinitionParser.GetFieldsUsingControl(DEFINITION, "Kentico.Administration.Missing"), Is.Empty);

    [TestCase(null)]
    [TestCase("")]
    public void GetFieldsUsingControl_MissingXml_ReturnsEmpty(string? xml) => Assert.That(ClassFormDefinitionParser.GetFieldsUsingControl(xml, "Kentico.Administration.TextInput"), Is.Empty);

    [Test]
    public void GetFieldsUsingControl_SchemaField_ReturnsSchemaIdentifierAndName()
    {
        const string schemaDefinition = """
            <form>
              <schema guid="7c7b596d-5b91-40a5-8ded-f2e4757d8e66" name="SEOFields">
                <properties><fieldcaption>SEO fields</fieldcaption></properties>
              </schema>
              <field column="SEOFieldsTitle" guid="60b37ef3-d909-45d4-b5d1-fd27d9258396" columntype="text">
                <properties><kxp_schema_identifier>7C7B596D-5B91-40A5-8DED-F2E4757D8E66</kxp_schema_identifier></properties>
                <settings><controlname>Kentico.Administration.TextInput</controlname></settings>
              </field>
            </form>
            """;

        var field = ClassFormDefinitionParser.GetFieldsUsingControl(schemaDefinition, "Kentico.Administration.TextInput").Single();

        Assert.Multiple(() =>
        {
            Assert.That(field.SchemaIdentifier, Is.EqualTo("7C7B596D-5B91-40A5-8DED-F2E4757D8E66"));
            Assert.That(field.SchemaName, Is.EqualTo("SEO fields"));
            Assert.That(field.SchemaCodeName, Is.EqualTo("SEOFields"));
        });
    }

    [Test]
    public void GetFieldsUsingControl_InvalidXml_Throws()
        => Assert.Throws<XmlException>(() => ClassFormDefinitionParser.GetFieldsUsingControl("<form><field", "Kentico.Administration.TextInput"));
}
