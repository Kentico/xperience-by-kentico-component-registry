using System.Data;
using System.Data.Common;
using System.Xml.Linq;

using CMS.DataEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

namespace Kentico.Xperience.Labs.ComponentRegistry;

public partial class ComponentUsageService
{
    /// <summary>
    /// <see cref="AdminFormComponentClassUsageDto.ClassType"/> of usage entries representing a reusable field schema.
    /// </summary>
    public const string REUSABLE_FIELD_SCHEMA_CLASS_TYPE = "ReusableFieldSchema";

    private const string ADMIN_FORM_COMPONENT_CLASS_QUERY = @"
        SELECT
            C.ClassID,
            C.ClassName,
            C.ClassDisplayName,
            C.ClassType,
            C.ClassResourceID,
            R.ResourceName,
            C.ClassLastModified,
            C.ClassFormDefinition AS FormDefinition
        FROM CMS_Class C
        LEFT JOIN CMS_Resource R ON R.ResourceID = C.ClassResourceID
        WHERE C.ClassFormDefinition IS NOT NULL
            AND CHARINDEX(@controlName, C.ClassFormDefinition) > 0";

    // Object type editing UIs (custom and system module classes) are stored as UI forms, not in the class definition.
    private const string ADMIN_FORM_COMPONENT_UI_FORM_QUERY = @"
        SELECT
            C.ClassID,
            C.ClassName,
            C.ClassDisplayName,
            C.ClassType,
            C.ClassResourceID,
            R.ResourceName,
            C.ClassLastModified,
            F.FormID,
            F.FormName,
            F.FormDisplayName,
            F.FormLastModified,
            F.FormDefinition
        FROM CMS_AlternativeForm F
        INNER JOIN CMS_Class C ON C.ClassID = F.FormClassID
        LEFT JOIN CMS_Resource R ON R.ResourceID = C.ClassResourceID
        WHERE F.FormDefinition IS NOT NULL
            AND CHARINDEX(@controlName, F.FormDefinition) > 0";

    /// <inheritdoc/>
    public async Task<AdminFormComponentUsageDetailDto> GetAdminFormComponentUsageAsync(string componentIdentifier)
    {
        var result = new AdminFormComponentUsageDetailDto { ComponentIdentifier = componentIdentifier };
        var classes = new Dictionary<int, (AdminFormComponentClassUsageDto Usage, int? ModuleId, string? ModuleName)>();
        var schemaUsages = new List<AdminFormComponentClassUsageDto>();

        async Task<(AdminFormComponentClassUsageDto Usage, int? ModuleId, string? ModuleName)> GetOrAddClass(DbDataReader reader)
        {
            int classId = reader.GetInt32(reader.GetOrdinal("ClassID"));
            if (!classes.TryGetValue(classId, out var entry))
            {
                int moduleOrdinal = reader.GetOrdinal("ClassResourceID");
                entry = (
                    new AdminFormComponentClassUsageDto
                    {
                        ClassId = classId,
                        ClassName = await GetNullableString(reader, "ClassName", default) ?? string.Empty,
                        ClassDisplayName = await GetNullableString(reader, "ClassDisplayName", default) ?? string.Empty,
                        ClassType = await GetNullableString(reader, "ClassType", default),
                        ClassLastModified = await GetNullableDateTime(reader, "ClassLastModified", default),
                    },
                    await reader.IsDBNullAsync(moduleOrdinal) ? null : reader.GetInt32(moduleOrdinal),
                    await GetNullableString(reader, "ResourceName", default));
                classes[classId] = entry;
            }

            return entry;
        }

        using var connection = ConnectionHelper.GetConnection();

        // SQL pre-filter only; the parser still performs the exact match
        QueryDataParameters CreateParameters() => new() { { "@controlName", componentIdentifier } };

        var classReader = await connection.ExecuteReaderAsync(
            new QueryParameters(ADMIN_FORM_COMPONENT_CLASS_QUERY, CreateParameters(), QueryTypeEnum.SQLQuery), CommandBehavior.Default, default);
        if (classReader is not null)
        {
            await using (classReader)
            {
                while (await classReader.ReadAsync())
                {
                    var fields = ClassFormDefinitionParser.GetFieldsUsingControl(
                        await GetNullableString(classReader, "FormDefinition", default), componentIdentifier);
                    if (fields.Count == 0)
                    {
                        continue;
                    }

                    var classFields = fields.Where(f => f.SchemaIdentifier is null).ToList();
                    if (classFields.Count > 0)
                    {
                        (await GetOrAddClass(classReader)).Usage.Fields.AddRange(classFields);
                    }

                    // Reusable field schema fields are stored in CMS.ContentItemCommonData; report them per schema instead.
                    foreach (var schemaFields in fields.Where(f => f.SchemaIdentifier is not null).GroupBy(f => f.SchemaIdentifier!))
                    {
                        var first = schemaFields.First();
                        schemaUsages.Add(new AdminFormComponentClassUsageDto
                        {
                            ClassId = classReader.GetInt32(classReader.GetOrdinal("ClassID")),
                            ClassName = first.SchemaCodeName ?? schemaFields.Key,
                            ClassDisplayName = first.SchemaName ?? first.SchemaCodeName ?? schemaFields.Key,
                            ClassType = REUSABLE_FIELD_SCHEMA_CLASS_TYPE,
                            ClassLastModified = await GetNullableDateTime(classReader, "ClassLastModified", default),
                            ReusableFieldSchemaIdentifier = schemaFields.Key,
                            Fields = [.. schemaFields],
                        });
                    }
                }
            }
        }

        var uiFormReader = await connection.ExecuteReaderAsync(
            new QueryParameters(ADMIN_FORM_COMPONENT_UI_FORM_QUERY, CreateParameters(), QueryTypeEnum.SQLQuery), CommandBehavior.Default, default);
        if (uiFormReader is not null)
        {
            await using (uiFormReader)
            {
                while (await uiFormReader.ReadAsync())
                {
                    var fields = ClassFormDefinitionParser.GetFieldsUsingControl(
                        await GetNullableString(uiFormReader, "FormDefinition", default), componentIdentifier);
                    if (fields.Count == 0)
                    {
                        continue;
                    }

                    (await GetOrAddClass(uiFormReader)).Usage.UIForms.Add(new AdminFormComponentUIFormUsageDto
                    {
                        FormId = uiFormReader.GetInt32(uiFormReader.GetOrdinal("FormID")),
                        FormName = await GetNullableString(uiFormReader, "FormName", default) ?? string.Empty,
                        FormDisplayName = await GetNullableString(uiFormReader, "FormDisplayName", default) ?? string.Empty,
                        FormLastModified = await GetNullableDateTime(uiFormReader, "FormLastModified", default),
                        Fields = fields,
                    });
                }
            }
        }

        foreach (var (usage, moduleId, moduleName) in classes.Values)
        {
            SetAdminFormComponentAdminPaths(usage, moduleId, moduleName);
        }

        foreach (var schemaUsage in schemaUsages)
        {
            if (Guid.TryParse(schemaUsage.ReusableFieldSchemaIdentifier, out var schemaGuid))
            {
                schemaUsage.AdminPath = ToRelativeAdminPath(pageLinkGenerator.GetPath<ReusableFieldSchemaFields>(
                    new PageParameterValues()
                    {
                        { typeof(ReusableFieldSchemaEditSection), schemaGuid },
                    }));
            }
        }

        result.Classes = classes.Values
            .Select(entry => entry.Usage)
            .Concat(schemaUsages)
            .OrderBy(c => c.ClassDisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.ClassName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        result.TotalClassesUsing = result.Classes.Count;
        result.TotalFieldsUsing = result.Classes.Sum(c => c.Fields.Count + c.UIForms.Sum(f => f.Fields.Count));
        result.LastModified = result.Classes
            .SelectMany(c => c.UIForms.Select(f => f.FormLastModified).Append(c.ClassLastModified))
            .Max();

        return result;
    }

    private void SetAdminFormComponentAdminPaths(AdminFormComponentClassUsageDto usedClass, int? moduleId, string? moduleName)
    {
        if (string.Equals(usedClass.ClassType, ClassType.CONTENT_TYPE, StringComparison.OrdinalIgnoreCase))
        {
            usedClass.AdminPath = ToRelativeAdminPath(pageLinkGenerator.GetPath<ContentTypeFields>(
                new PageParameterValues()
                {
                    { typeof(ContentTypeEditSection), usedClass.ClassId },
                }));
        }
        else if (moduleId is int id && IsCustomModule(moduleName))
        {
            foreach (var uiForm in usedClass.UIForms)
            {
                uiForm.AdminPath = ToRelativeAdminPath(pageLinkGenerator.GetPath(
                    uiFormFieldsPageType,
                    new PageParameterValues()
                    {
                        { typeof(ModuleEditSection), id },
                        { typeof(ClassEditSection), usedClass.ClassId },
                        { typeof(UIFormEditSection), uiForm.FormId },
                    }));
            }
        }
    }

    private static string ToRelativeAdminPath(string adminPath) =>
        adminPath.StartsWith('/') ? adminPath[1..] : adminPath;

#pragma warning disable S1135
    // TODO: Resolve UIFormFields directly when Xperience makes the UI form field editor tab public.
#pragma warning restore S1135
    private static readonly Type uiFormFieldsPageType = typeof(UIFormEditSection).Assembly
        .GetType("Kentico.Xperience.Admin.Base.UIPages.UIFormFields")
        ?? throw new TypeLoadException("The UI form fields admin page type could not be found.");

    // System modules (CMS, CMS.*) are not editable in the Modules application.
    private static bool IsCustomModule(string? moduleName) =>
        !string.IsNullOrEmpty(moduleName)
        && !string.Equals(moduleName, "CMS", StringComparison.OrdinalIgnoreCase)
        && !moduleName.StartsWith("CMS.", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Parses <c>CMS_Class.ClassFormDefinition</c> XML to find fields assigned a form component through <c>field/settings/controlname</c>.
/// </summary>
internal static class ClassFormDefinitionParser
{
    public static List<AdminFormComponentFieldUsageDto> GetFieldsUsingControl(string? formDefinition, string controlName)
    {
        if (string.IsNullOrWhiteSpace(formDefinition))
        {
            return [];
        }

        var root = XElement.Parse(formDefinition);

        var schemas = root.Elements("schema")
            .Select(schema => (
                Guid: ((string?)schema.Attribute("guid"))?.Trim(),
                CodeName: NullIfWhiteSpace((string?)schema.Attribute("name")),
                Name: NullIfWhiteSpace(schema.Element("properties")?.Element("fieldcaption")?.Value)))
            .Where(schema => schema.Guid is not null)
            .GroupBy(schema => schema.Guid!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        return root.Elements("field")
            .Where(field => string.Equals(
                field.Element("settings")?.Element("controlname")?.Value.Trim(),
                controlName,
                StringComparison.OrdinalIgnoreCase))
            .Select(field =>
            {
                var properties = field.Element("properties");
                string? schemaIdentifier = NullIfWhiteSpace(properties?.Element("kxp_schema_identifier")?.Value);
                bool hasSchema = schemaIdentifier is not null && schemas.ContainsKey(schemaIdentifier);

                return new AdminFormComponentFieldUsageDto
                {
                    FieldName = (string?)field.Attribute("column") ?? string.Empty,
                    FieldGuid = (string?)field.Attribute("guid"),
                    FieldCaption = NullIfWhiteSpace(properties?.Element("fieldcaption")?.Value),
                    SchemaIdentifier = schemaIdentifier,
                    SchemaCodeName = hasSchema ? schemas[schemaIdentifier!].CodeName : null,
                    SchemaName = hasSchema ? schemas[schemaIdentifier!].Name ?? schemas[schemaIdentifier!].CodeName : null,
                };
            })
            .ToList();
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>
/// Represents class fields assigned an Admin UI form component.
/// </summary>
public class AdminFormComponentUsageDetailDto
{
    /// <summary>
    /// The identifier of the Admin UI form component.
    /// </summary>
    public string ComponentIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Number of classes with at least one field using the component.
    /// </summary>
    public int TotalClassesUsing { get; set; }

    /// <summary>
    /// Number of fields using the component across all classes.
    /// </summary>
    public int TotalFieldsUsing { get; set; }

    /// <summary>
    /// Most recent modification of a matching class.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Classes with fields using the component, based on persisted form definitions only.
    /// </summary>
    public List<AdminFormComponentClassUsageDto> Classes { get; set; } = [];
}

/// <summary>
/// A class (<c>CMS_Class</c>) with fields assigned an Admin UI form component.
/// </summary>
public class AdminFormComponentClassUsageDto
{
    /// <summary>
    /// The ID of the class.
    /// </summary>
    public int ClassId { get; set; }

    /// <summary>
    /// The code name of the class.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the class.
    /// </summary>
    public string ClassDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The class type, for example Content or Form.
    /// </summary>
    public string? ClassType { get; set; }

    /// <summary>
    /// When the class was last modified (<c>CMS_Class.ClassLastModified</c>).
    /// </summary>
    public DateTime? ClassLastModified { get; set; }

    /// <summary>
    /// Relative administration path to the content type or reusable field schema fields.
    /// </summary>
    public string? AdminPath { get; set; }

    /// <summary>
    /// GUID of the reusable field schema when this entry represents a schema
    /// (<see cref="ClassType"/> is <c>ReusableFieldSchema</c>); <see cref="ClassId"/> is then the
    /// <c>CMS.ContentItemCommonData</c> class storing the schema fields.
    /// </summary>
    public string? ReusableFieldSchemaIdentifier { get; set; }

    /// <summary>
    /// Fields of the class definition assigned the component.
    /// </summary>
    public List<AdminFormComponentFieldUsageDto> Fields { get; set; } = [];

    /// <summary>
    /// UI forms (<c>CMS_AlternativeForm</c>) of the class with fields assigned the component.
    /// Object type editing UIs are defined here rather than in the class definition.
    /// </summary>
    public List<AdminFormComponentUIFormUsageDto> UIForms { get; set; } = [];
}

/// <summary>
/// A class UI form (<c>CMS_AlternativeForm</c>) with fields assigned an Admin UI form component.
/// </summary>
public class AdminFormComponentUIFormUsageDto
{
    /// <summary>
    /// The ID of the UI form.
    /// </summary>
    public int FormId { get; set; }

    /// <summary>
    /// The code name of the UI form.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the UI form.
    /// </summary>
    public string FormDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// When the UI form was last modified.
    /// </summary>
    public DateTime? FormLastModified { get; set; }

    /// <summary>
    /// Relative administration path to the UI form fields, set for custom module classes only.
    /// </summary>
    public string? AdminPath { get; set; }

    /// <summary>
    /// Fields of the UI form assigned the component.
    /// </summary>
    public List<AdminFormComponentFieldUsageDto> Fields { get; set; } = [];
}

/// <summary>
/// A class field assigned an Admin UI form component.
/// </summary>
public class AdminFormComponentFieldUsageDto
{
    /// <summary>
    /// The field column name.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// The field GUID.
    /// </summary>
    public string? FieldGuid { get; set; }

    /// <summary>
    /// The field caption as stored in the form definition, which may be a localization macro.
    /// </summary>
    public string? FieldCaption { get; set; }

    /// <summary>
    /// GUID of the reusable field schema the field belongs to (<c>properties/kxp_schema_identifier</c>), if any.
    /// </summary>
    public string? SchemaIdentifier { get; set; }

    /// <summary>
    /// Display name of the reusable field schema the field belongs to, if any.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Code name of the reusable field schema the field belongs to, if any.
    /// </summary>
    public string? SchemaCodeName { get; set; }
}
