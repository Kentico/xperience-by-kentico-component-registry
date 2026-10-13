using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

using CMS.DataEngine;

using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.Labs.ComponentRegistry;

public partial class ComponentUsageService
{
    // CMS_Workflow.WorkflowType value of automation processes
    private const int AUTOMATION_WORKFLOW_TYPE = 3;

    private const string AUTOMATION_PROCESS_COLUMNS = @"
        W.WorkflowID,
        W.WorkflowDisplayName,
        W.WorkflowName,
        W.WorkflowEnabled,
        W.WorkflowLastModified";

    private static readonly string automationTriggerQuery = $@"
        SELECT
            {AUTOMATION_PROCESS_COLUMNS},
            T.TriggerID AS UsageID,
            CAST(NULL AS nvarchar(450)) AS UsageName,
            T.TriggerDisplayName AS UsageDisplayName,
            T.TriggerParameters AS UsageParameters,
            T.TriggerLastModified AS UsageLastModified
        FROM CMS_ObjectWorkflowTrigger T
        INNER JOIN CMS_Workflow W ON W.WorkflowID = T.TriggerWorkflowID
        WHERE W.WorkflowType = @workflowType
            AND T.TriggerParameters LIKE @parametersPattern ESCAPE '\'
        ORDER BY W.WorkflowDisplayName, T.TriggerDisplayName";

    private static readonly string automationStepQuery = $@"
        SELECT
            {AUTOMATION_PROCESS_COLUMNS},
            S.StepID AS UsageID,
            S.StepName AS UsageName,
            S.StepDisplayName AS UsageDisplayName,
            S.StepActionParameters AS UsageParameters,
            S.StepLastModified AS UsageLastModified
        FROM CMS_WorkflowStep S
        INNER JOIN CMS_Workflow W ON W.WorkflowID = S.StepWorkflowID
        WHERE W.WorkflowType = @workflowType
            AND S.StepActionParameters LIKE @parametersPattern ESCAPE '\'
        ORDER BY W.WorkflowDisplayName, S.StepDisplayName";

    /// <inheritdoc/>
    public Task<AutomationStepUsageDetailDto> GetAutomationTriggerUsageAsync(string triggerIdentifier) =>
        GetAutomationUsage(triggerIdentifier, AutomationStepKind.Trigger);

    /// <inheritdoc/>
    public Task<AutomationStepUsageDetailDto> GetAutomationActionUsageAsync(string actionIdentifier) =>
        GetAutomationUsage(actionIdentifier, AutomationStepKind.Action);

    /// <inheritdoc/>
    public Task<AutomationStepUsageDetailDto> GetAutomationConditionUsageAsync(string conditionIdentifier) =>
        GetAutomationUsage(conditionIdentifier, AutomationStepKind.Condition);

    /// <summary>
    /// Retrieves automation processes referencing a custom step. Rows are pre-filtered with LIKE
    /// and then matched exactly by parsing the step/trigger parameters XML.
    /// </summary>
    private async Task<AutomationStepUsageDetailDto> GetAutomationUsage(
        string identifier,
        AutomationStepKind kind,
        CancellationToken cancellationToken = default)
    {
        var result = new AutomationStepUsageDetailDto
        {
            ComponentIdentifier = identifier,
            ComponentType = kind.ToString(),
        };

        try
        {
            string identifierElement = AutomationStepParameters.GetIdentifierElement(kind);
            string query = kind == AutomationStepKind.Trigger ? automationTriggerQuery : automationStepQuery;
            var parameters = new QueryDataParameters
            {
                { "@workflowType", AUTOMATION_WORKFLOW_TYPE },
                { "@parametersPattern", $"%<{identifierElement}>%{EscapeForLike(identifier)}%" }
            };

            var processes = new Dictionary<int, AutomationProcessUsageDto>();

            using (var connection = ConnectionHelper.GetConnection())
            {
                var reader = await connection.ExecuteReaderAsync(
                    new QueryParameters(query, parameters, QueryTypeEnum.SQLQuery),
                    CommandBehavior.Default,
                    cancellationToken);

                if (reader is not null)
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        string? parametersXml = await GetNullableString(reader, "UsageParameters", cancellationToken);
                        if (!AutomationStepParameters.TryParse(parametersXml, identifierElement, identifier, out var stepParameters))
                        {
                            continue;
                        }

                        int processId = reader.GetInt32(reader.GetOrdinal("WorkflowID"));
                        if (!processes.TryGetValue(processId, out var process))
                        {
                            process = new AutomationProcessUsageDto
                            {
                                ProcessID = processId,
                                ProcessDisplayName = await GetNullableString(reader, "WorkflowDisplayName", cancellationToken) ?? string.Empty,
                                ProcessName = await GetNullableString(reader, "WorkflowName", cancellationToken) ?? string.Empty,
                                IsEnabled = await GetNullableBoolean(reader, "WorkflowEnabled", cancellationToken) ?? false,
                                LastModified = await GetNullableDateTime(reader, "WorkflowLastModified", cancellationToken),
                            };
                            processes.Add(processId, process);
                        }

                        string displayName = await GetNullableString(reader, "UsageDisplayName", cancellationToken) ?? string.Empty;

                        process.Usages.Add(new AutomationProcessStepUsageDto
                        {
                            ObjectID = reader.GetInt32(reader.GetOrdinal("UsageID")),
                            Name = await GetNullableString(reader, "UsageName", cancellationToken),
                            DisplayName = kind == AutomationStepKind.Trigger
                                ? localizationService.LocalizeResourceKey(displayName)
                                : displayName,
                            LastModified = await GetNullableDateTime(reader, "UsageLastModified", cancellationToken),
                            Parameters = stepParameters,
                        });
                    }
                }
            }

            result.Processes = processes.Values.ToList();
            result.TotalProcessesUsing = result.Processes.Count;
            result.TotalUsages = result.Processes.Sum(p => p.Usages.Count);
            result.LastModified = result.Processes
                .SelectMany(p => p.Usages.Select(u => u.LastModified).Append(p.LastModified))
                .Where(d => d.HasValue)
                .DefaultIfEmpty()
                .Max();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving automation {StepKind} usage for {ComponentIdentifier}", kind, identifier);
            return result;
        }
    }

    private static async Task<string?> GetNullableString(DbDataReader reader, string column, CancellationToken cancellationToken)
    {
        int ordinal = reader.GetOrdinal(column);
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetString(ordinal);
    }

    private static async Task<bool?> GetNullableBoolean(DbDataReader reader, string column, CancellationToken cancellationToken)
    {
        int ordinal = reader.GetOrdinal(column);
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetBoolean(ordinal);
    }

    private static async Task<DateTime?> GetNullableDateTime(DbDataReader reader, string column, CancellationToken cancellationToken)
    {
        int ordinal = reader.GetOrdinal(column);
        return await reader.IsDBNullAsync(ordinal, cancellationToken) ? null : reader.GetDateTime(ordinal);
    }
}

/// <summary>
/// Kind of custom automation step.
/// </summary>
public enum AutomationStepKind
{
    /// <summary>Custom automation trigger.</summary>
    Trigger,

    /// <summary>Custom automation action.</summary>
    Action,

    /// <summary>Custom automation condition.</summary>
    Condition
}

/// <summary>
/// Parses the XML parameters stored in <c>CMS_WorkflowStep.StepActionParameters</c> and <c>CMS_ObjectWorkflowTrigger.TriggerParameters</c>.
/// </summary>
internal static class AutomationStepParameters
{
    public const string TRIGGER_IDENTIFIER_ELEMENT = "CustomTrigger";
    public const string ACTION_IDENTIFIER_ELEMENT = "CustomActionIdentifier";
    public const string CONDITION_IDENTIFIER_ELEMENT = "CustomConditionIdentifier";

    public static string GetIdentifierElement(AutomationStepKind kind) => kind switch
    {
        AutomationStepKind.Trigger => TRIGGER_IDENTIFIER_ELEMENT,
        AutomationStepKind.Action => ACTION_IDENTIFIER_ELEMENT,
        AutomationStepKind.Condition => CONDITION_IDENTIFIER_ELEMENT,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    /// <summary>
    /// Returns <c>true</c> when the identifier element matches <paramref name="identifier"/>;
    /// <paramref name="parameters"/> then holds the remaining configured parameters.
    /// </summary>
    public static bool TryParse(
        string? xml,
        string identifierElement,
        string identifier,
        out List<AutomationStepParameterDto> parameters)
    {
        parameters = [];

        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        XElement root;
        try
        {
            root = XElement.Parse(xml);
        }
        catch (XmlException)
        {
            return false;
        }

        string? storedIdentifier = root.Element(identifierElement)?.Value.Trim();
        if (!string.Equals(storedIdentifier, identifier, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        parameters = root.Elements()
            .Where(e => e.Name.LocalName != identifierElement)
            .Select(e => new AutomationStepParameterDto
            {
                Name = e.Name.LocalName,
                Value = FormatValue(e.Value.Trim())
            })
            .ToList();

        return true;
    }

    // Values are stored as JSON (strings quoted, arrays/objects/booleans raw)
    private static string FormatValue(string value)
    {
        if (value.Length < 2 || value[0] != '"' || value[^1] != '"')
        {
            return value;
        }

        try
        {
            return JsonSerializer.Deserialize<string>(value) ?? string.Empty;
        }
        catch (JsonException)
        {
            return value;
        }
    }
}

/// <summary>
/// Represents automation process usage for a custom automation trigger, action, or condition.
/// </summary>
public class AutomationStepUsageDetailDto
{
    /// <summary>
    /// The unique identifier of the custom automation step.
    /// </summary>
    public string ComponentIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The kind of step ("Trigger", "Action", or "Condition").
    /// </summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Number of automation processes using this step.
    /// </summary>
    public int TotalProcessesUsing { get; set; }

    /// <summary>
    /// Number of triggers or steps using this step across all automation processes.
    /// </summary>
    public int TotalUsages { get; set; }

    /// <summary>
    /// Most recent modification of a matching process, step, or trigger.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Automation processes using this step.
    /// </summary>
    public List<AutomationProcessUsageDto> Processes { get; set; } = [];
}

/// <summary>
/// An automation process using a custom automation step.
/// </summary>
public class AutomationProcessUsageDto
{
    /// <summary>
    /// Automation process (CMS_Workflow) ID.
    /// </summary>
    public int ProcessID { get; set; }

    /// <summary>
    /// Automation process display name.
    /// </summary>
    public string ProcessDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Automation process code name.
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the automation process is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Last modification of the automation process.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Admin path to the automation process builder.
    /// </summary>
    public string? AdminPath { get; set; }

    /// <summary>
    /// Triggers or steps in this process using the custom automation step.
    /// </summary>
    public List<AutomationProcessStepUsageDto> Usages { get; set; } = [];
}

/// <summary>
/// A trigger or step of an automation process using a custom automation step.
/// </summary>
public class AutomationProcessStepUsageDto
{
    /// <summary>
    /// Trigger (CMS_ObjectWorkflowTrigger) or step (CMS_WorkflowStep) ID.
    /// </summary>
    public int ObjectID { get; set; }

    /// <summary>
    /// Step code name. Null for triggers.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Trigger or step display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Last modification of the trigger or step.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Configured parameters, excluding the step identifier.
    /// </summary>
    public List<AutomationStepParameterDto> Parameters { get; set; } = [];
}

/// <summary>
/// A configured parameter of an automation trigger or step.
/// </summary>
public class AutomationStepParameterDto
{
    /// <summary>
    /// Parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parameter value. JSON string values are unquoted; other JSON values are kept as-is.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
