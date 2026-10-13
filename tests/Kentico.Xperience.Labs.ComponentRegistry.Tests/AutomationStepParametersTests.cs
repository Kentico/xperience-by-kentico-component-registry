namespace Kentico.Xperience.Labs.ComponentRegistry.Tests;

public class AutomationStepParametersTests
{
    private const string ACTION_XML = """
        <Parameters>
            <CustomActionIdentifier>
                DancingGoat.CoffeeClubEnrollment
            </CustomActionIdentifier>
            <Outcome>
                "automatic"
            </Outcome>
            <RecordResult>
                true
            </RecordResult>
        </Parameters>
        """;

    private const string CONDITION_XML = """
        <Parameters>
            <CustomConditionIdentifier>
                Labs.AutomationExperiments.PreviousStepSucceeded
            </CustomConditionIdentifier>
            <SourceName>
                "DancingGoat.CoffeeClubEnrollment"
            </SourceName>
        </Parameters>
        """;

    [Test]
    public void TryParse_MatchingIdentifier_ReturnsTrimmedUnquotedParameters()
    {
        bool matched = AutomationStepParameters.TryParse(
            ACTION_XML, AutomationStepParameters.ACTION_IDENTIFIER_ELEMENT, "dancinggoat.coffeeclubenrollment", out var parameters);

        Assert.That(matched, Is.True);
        Assert.That(parameters.Select(p => (p.Name, p.Value)), Is.EqualTo(new[]
        {
            ("Outcome", "automatic"),
            ("RecordResult", "true")
        }));
    }

    [Test]
    public void TryParse_IdentifierOnlyInOtherParameter_ReturnsFalse()
    {
        bool matched = AutomationStepParameters.TryParse(
            CONDITION_XML, AutomationStepParameters.ACTION_IDENTIFIER_ELEMENT, "DancingGoat.CoffeeClubEnrollment", out var parameters);

        Assert.That(matched, Is.False);
        Assert.That(parameters, Is.Empty);
    }

    [Test]
    public void TryParse_InvalidXml_ReturnsFalse() =>
        Assert.That(AutomationStepParameters.TryParse(
            "<Parameters>", AutomationStepParameters.TRIGGER_IDENTIFIER_ELEMENT, "x", out _), Is.False);
}
