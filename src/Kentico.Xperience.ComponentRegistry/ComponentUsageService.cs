using System.Data;

using CMS.ContentEngine;
using CMS.DataEngine;

using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.ComponentRegistry;

/// <summary>
/// Service for retrieving component usage information across the site.
/// </summary>
public interface IComponentUsageService
{
    /// <summary>
    /// Gets detailed usage information for a specific page builder page template.
    /// </summary>
    /// <param name="templateIdentifier">The unique identifier of the page template.</param>
    /// <returns>Component usage details including all pages and language variants.</returns>
    public Task<ComponentUsageDetailDto> GetPageBuilderPageTemplateUsageAsync(string templateIdentifier);

    /// <summary>
    /// Gets detailed usage information for a specific page builder widget.
    /// </summary>
    /// <param name="widgetIdentifier">The unique identifier of the widget.</param>
    /// <returns>Component usage details including all pages and language variants.</returns>
    public Task<ComponentUsageDetailDto> GetPageBuilderWidgetUsageAsync(string widgetIdentifier);

    /// <summary>
    /// Gets usage information for multiple components in a batch operation.
    /// </summary>
    /// <param name="identifiers">List of component identifiers to query.</param>
    /// <param name="componentType">The type of components being queried ("PageTemplate" or "Widget").</param>
    /// <returns>List of component usage details for each identifier.</returns>
    public Task<List<ComponentUsageDetailDto>> GetBatchUsageAsync(List<string> identifiers, string componentType);

    /// <summary>
    /// Gets detailed usage information for a specific email builder widget.
    /// </summary>
    /// <param name="widgetIdentifier">The unique identifier of the email widget.</param>
    /// <returns>Email configuration usage details including all email configurations and language variants.</returns>
    public Task<EmailConfigurationUsageDetailDto> GetEmailBuilderWidgetUsageAsync(string widgetIdentifier);

    /// <summary>
    /// Gets detailed usage information for a specific email builder template.
    /// </summary>
    /// <param name="templateIdentifier">The unique identifier of the email template.</param>
    /// <returns>Email configuration usage details including all email configurations and language variants.</returns>
    public Task<EmailConfigurationUsageDetailDto> GetEmailBuilderTemplateUsageAsync(string templateIdentifier);

    /// <summary>
    /// Gets detailed usage information for a specific form builder component or section.
    /// </summary>
    /// <param name="componentIdentifier">The unique identifier of the form component or section.</param>
    /// <returns>Form class usage details including all form classes using this component.</returns>
    public Task<FormComponentUsageDetailDto> GetFormBuilderComponentUsageAsync(string componentIdentifier);

    /// <summary>
    /// Gets detailed usage information for a specific form builder section.
    /// </summary>
    /// <param name="sectionIdentifier">The unique identifier of the form section.</param>
    /// <returns>Form class usage details including all form classes using this section.</returns>
    public Task<FormComponentUsageDetailDto> GetFormBuilderSectionUsageAsync(string sectionIdentifier);
}


/// <summary>
/// Service for retrieving component usage information from the database.
/// Uses CMS.DataEngine.ConnectionHelper to query CMS_WebPageItem and CMS_ContentItemCommonData.
/// </summary>
public class ComponentUsageService(ILogger<ComponentUsageService> logger) : IComponentUsageService
{
    private readonly ILogger<ComponentUsageService> logger = logger;

    /// <inheritdoc/>
    public Task<ComponentUsageDetailDto> GetPageBuilderPageTemplateUsageAsync(string templateIdentifier) => GetComponentUsage(
            templateIdentifier,
            "PageTemplate",
            "ContentItemCommonDataVisualBuilderTemplateConfiguration");

    /// <inheritdoc/>
    public Task<ComponentUsageDetailDto> GetPageBuilderWidgetUsageAsync(string widgetIdentifier) => GetComponentUsage(
            widgetIdentifier,
            "Widget",
            "ContentItemCommonDataVisualBuilderWidgets");

    /// <inheritdoc/>
    public async Task<List<ComponentUsageDetailDto>> GetBatchUsageAsync(
        List<string> identifiers,
        string componentType)
    {
        var results = new List<ComponentUsageDetailDto>();

        foreach (string identifier in identifiers)
        {
            var usage = componentType.Equals("PageTemplate", StringComparison.InvariantCultureIgnoreCase)
                ? await GetPageBuilderPageTemplateUsageAsync(identifier)
                : await GetPageBuilderWidgetUsageAsync(identifier);

            results.Add(usage);
        }

        return results;
    }

    /// <inheritdoc/>
    public Task<EmailConfigurationUsageDetailDto> GetEmailBuilderWidgetUsageAsync(string widgetIdentifier) => GetEmailComponentUsage(
            widgetIdentifier,
            "EmailWidget",
            "ContentItemCommonDataVisualBuilderWidgets");

    /// <inheritdoc/>
    public Task<EmailConfigurationUsageDetailDto> GetEmailBuilderTemplateUsageAsync(string templateIdentifier) => GetEmailComponentUsage(
            templateIdentifier,
            "EmailTemplate",
            "ContentItemCommonDataVisualBuilderTemplateConfiguration");

    /// <inheritdoc/>
    public Task<FormComponentUsageDetailDto> GetFormBuilderComponentUsageAsync(string componentIdentifier) =>
        GetFormComponentUsage(componentIdentifier, isSection: false);

    /// <inheritdoc/>
    public Task<FormComponentUsageDetailDto> GetFormBuilderSectionUsageAsync(string sectionIdentifier) =>
        GetFormComponentUsage(sectionIdentifier, isSection: true);

    /// <summary>
    /// Retrieves component usage by querying the database.
    /// </summary>
    private async Task<ComponentUsageDetailDto> GetComponentUsage(
        string componentIdentifier,
        string componentType,
        string configColumnName,
        CancellationToken cancellationToken = default)
    {
        var result = new ComponentUsageDetailDto
        {
            ComponentIdentifier = componentIdentifier,
            ComponentType = componentType,
            Pages = []
        };

        try
        {
            string query = @"
                SELECT
                    WPI.WebPageItemID,
                    WPI.WebPageItemName,
                    WPI.WebPageItemTreePath,
                    WPI.WebPageItemWebsiteChannelID,
                    C.ChannelDisplayName,
                    CI.ContentItemID,
                    CL.ContentLanguageID,
                    CL.ContentLanguageName,
                    CICD.ContentItemCommonDataID,
                    CICD.ContentItemCommonDataVersionStatus,
                    CICD.ContentItemCommonDataLastPublishedWhen,
                    CICD.[{configColumnName}]
                FROM CMS_WebPageItem WPI
                INNER JOIN CMS_ContentItem CI ON WPI.WebPageItemContentItemID = CI.ContentItemID
                INNER JOIN CMS_ContentItemCommonData CICD ON CI.ContentItemID = CICD.ContentItemCommonDataContentItemID
                INNER JOIN CMS_ContentLanguage CL ON CICD.ContentItemCommonDataContentLanguageID = CL.ContentLanguageID
                INNER JOIN CMS_Channel C ON CI.ContentItemChannelID = C.ChannelID
                WHERE CICD.[{configColumnName}] LIKE @componentId ESCAPE '\'
                ORDER BY WPI.WebPageItemName, CL.ContentLanguageName";

            query = query.Replace("{configColumnName}", configColumnName);

            var dataParameters = new QueryDataParameters
            {
                { "@componentId", $"%{EscapeForLike(componentIdentifier)}%" }
            };
            var parameters = new QueryParameters(query, dataParameters, QueryTypeEnum.SQLQuery);

            using (var connection = ConnectionHelper.GetConnection())
            {
                var reader = await connection.ExecuteReaderAsync(parameters, CommandBehavior.Default, cancellationToken);
                if (reader is null)
                {
                    return result;
                }

                var pagesByItem = new Dictionary<int, PageUsageDto>();
                DateTime? lastModified = null;

                while (await reader.ReadAsync(cancellationToken))
                {
                    int webPageItemId = reader.GetInt32(reader.GetOrdinal("WebPageItemID"));
                    string pageName = reader.GetString(reader.GetOrdinal("WebPageItemName"));
                    string pagePath = reader.GetString(reader.GetOrdinal("WebPageItemTreePath"));
                    string channelDisplayName = reader.GetString(reader.GetOrdinal("ChannelDisplayName"));
                    int websiteChannelID = reader.GetInt32(reader.GetOrdinal("WebPageItemWebsiteChannelID"));
                    int contentItemId = reader.GetInt32(reader.GetOrdinal("ContentItemID"));
                    string languageName = reader.GetString(reader.GetOrdinal("ContentLanguageName"));
                    int commonDataId = reader.GetInt32(reader.GetOrdinal("ContentItemCommonDataID"));
                    int versionStatusOrdinal = reader.GetOrdinal("ContentItemCommonDataVersionStatus");
                    int versionStatus = !await reader.IsDBNullAsync(versionStatusOrdinal, cancellationToken)
                        ? reader.GetInt32(versionStatusOrdinal)
                        : 0;
                    int modifiedWhenOrdinal = reader.GetOrdinal("ContentItemCommonDataLastPublishedWhen");
                    var modifiedWhen = !await reader.IsDBNullAsync(modifiedWhenOrdinal, cancellationToken)
                        ? reader.GetDateTime(modifiedWhenOrdinal)
                        : DateTime.UtcNow;
                    int configColumnOrdinal = reader.GetOrdinal(configColumnName);
                    string configJson = !await reader.IsDBNullAsync(configColumnOrdinal, cancellationToken)
                        ? reader.GetString(configColumnOrdinal)
                        : string.Empty;

                    if (lastModified == null || modifiedWhen > lastModified)
                    {
                        lastModified = modifiedWhen;
                    }

                    if (!pagesByItem.TryGetValue(webPageItemId, out var pageUsage))
                    {
                        pageUsage = new PageUsageDto
                        {
                            WebPageItemId = webPageItemId,
                            ContentItemId = contentItemId,
                            PageName = pageName,
                            PagePath = pagePath,
                            WebsiteChannelID = websiteChannelID,
                            ChannelDisplayName = channelDisplayName,
                            CreatedAt = DateTime.UtcNow,
                            Variants = []
                        };
                        pagesByItem[webPageItemId] = pageUsage;
                    }

                    pageUsage.Variants.Add(new PageVariantDto
                    {
                        ContentItemCommonDataId = commonDataId,
                        LanguageName = languageName,
                        LastModified = modifiedWhen,
                        ConfigurationJson = configJson,
                        ConfigurationType = componentType,
                        IsPublished = versionStatus == (int)VersionStatus.Published
                    });
                }

                result.Pages = pagesByItem.Values.OrderBy(p => p.PageName).ToList();
                result.TotalPagesUsing = result.Pages.Count;
                result.TotalVariants = result.Pages.SelectMany(p => p.Variants).Count();
                result.LastModified = lastModified;
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving component usage for {ComponentIdentifier}", componentIdentifier);
            return result;
        }
    }

    /// <summary>
    /// Retrieves email component usage by querying the database.
    /// </summary>
    private async Task<EmailConfigurationUsageDetailDto> GetEmailComponentUsage(
        string componentIdentifier,
        string componentType,
        string configColumnName,
        CancellationToken cancellationToken = default)
    {
        var result = new EmailConfigurationUsageDetailDto
        {
            ComponentIdentifier = componentIdentifier,
            ComponentType = componentType,
            EmailConfigurations = []
        };

        try
        {
            string query = @"
                SELECT
                    EC.EmailConfigurationID,
                    EC.EmailConfigurationEmailChannelID,
                    EC.EmailConfigurationName,
                    EC.EmailConfigurationPurpose,
                    C.ChannelDisplayName,
                    CI.ContentItemID,
                    CL.ContentLanguageID,
                    CL.ContentLanguageName,
                    CICD.ContentItemCommonDataID,
                    CICD.ContentItemCommonDataVersionStatus,
                    CICD.ContentItemCommonDataLastPublishedWhen,
                    CICD.[{configColumnName}]
                FROM EmailLibrary_EmailConfiguration EC
                INNER JOIN CMS_ContentItem CI ON EC.EmailConfigurationContentItemID = CI.ContentItemID
                INNER JOIN CMS_ContentItemCommonData CICD ON CI.ContentItemID = CICD.ContentItemCommonDataContentItemID
                INNER JOIN CMS_ContentLanguage CL ON CICD.ContentItemCommonDataContentLanguageID = CL.ContentLanguageID
                INNER JOIN CMS_Channel C ON CI.ContentItemChannelID = C.ChannelID
                WHERE CICD.[{configColumnName}] LIKE @componentId ESCAPE '\'
                ORDER BY EC.EmailConfigurationName, CL.ContentLanguageName";

            query = query.Replace("{configColumnName}", configColumnName);

            var dataParameters = new QueryDataParameters
            {
                { "@componentId", $"%{EscapeForLike(componentIdentifier)}%" }
            };
            var parameters = new QueryParameters(query, dataParameters, QueryTypeEnum.SQLQuery);

            using (var connection = ConnectionHelper.GetConnection())
            {
                var reader = await connection.ExecuteReaderAsync(parameters, CommandBehavior.Default, cancellationToken);
                if (reader is null)
                {
                    return result;
                }

                var configsByItem = new Dictionary<int, EmailConfigurationUsageDto>();
                DateTime? lastModified = null;

                while (await reader.ReadAsync(cancellationToken))
                {
                    int emailConfigurationId = reader.GetInt32(reader.GetOrdinal("EmailConfigurationID"));
                    string configurationName = reader.GetString(reader.GetOrdinal("EmailConfigurationName"));
                    string configurationPurpose = reader.GetString(reader.GetOrdinal("EmailConfigurationPurpose"));
                    int emailChannelID = reader.GetInt32(reader.GetOrdinal("EmailConfigurationEmailChannelID"));
                    string channelDisplayName = reader.GetString(reader.GetOrdinal("ChannelDisplayName"));
                    int contentItemId = reader.GetInt32(reader.GetOrdinal("ContentItemID"));
                    string languageName = reader.GetString(reader.GetOrdinal("ContentLanguageName"));
                    int commonDataId = reader.GetInt32(reader.GetOrdinal("ContentItemCommonDataID"));
                    int versionStatusOrdinal = reader.GetOrdinal("ContentItemCommonDataVersionStatus");
                    int versionStatus = !await reader.IsDBNullAsync(versionStatusOrdinal, cancellationToken)
                        ? reader.GetInt32(versionStatusOrdinal)
                        : 0;
                    int modifiedWhenOrdinal = reader.GetOrdinal("ContentItemCommonDataLastPublishedWhen");
                    var modifiedWhen = !await reader.IsDBNullAsync(modifiedWhenOrdinal, cancellationToken)
                        ? reader.GetDateTime(modifiedWhenOrdinal)
                        : DateTime.UtcNow;
                    int configColumnOrdinal = reader.GetOrdinal(configColumnName);
                    string configJson = !await reader.IsDBNullAsync(configColumnOrdinal, cancellationToken)
                        ? reader.GetString(configColumnOrdinal)
                        : string.Empty;

                    if (lastModified == null || modifiedWhen > lastModified)
                    {
                        lastModified = modifiedWhen;
                    }

                    if (!configsByItem.TryGetValue(emailConfigurationId, out var configUsage))
                    {
                        configUsage = new EmailConfigurationUsageDto
                        {
                            EmailConfigurationId = emailConfigurationId,
                            ContentItemId = contentItemId,
                            ConfigurationName = configurationName,
                            ConfigurationPurpose = configurationPurpose,
                            EmailChannelID = emailChannelID,
                            ChannelDisplayName = channelDisplayName,
                            CreatedAt = DateTime.UtcNow,
                            Variants = []
                        };
                        configsByItem[emailConfigurationId] = configUsage;
                    }

                    configUsage.Variants.Add(new EmailConfigurationVariantDto
                    {
                        ContentItemCommonDataId = commonDataId,
                        LanguageName = languageName,
                        LastModified = modifiedWhen,
                        ConfigurationJson = configJson,
                        ConfigurationType = componentType,
                        IsPublished = versionStatus == (int)VersionStatus.Published
                    });
                }

                result.EmailConfigurations = configsByItem.Values.OrderBy(c => c.ConfigurationName).ToList();
                result.TotalEmailConfigurationsUsing = result.EmailConfigurations.Count;
                result.TotalVariants = result.EmailConfigurations.SelectMany(c => c.Variants).Count();
                result.LastModified = lastModified;
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving email component usage for {ComponentIdentifier}", componentIdentifier);
            return result;
        }
    }

    /// <summary>
    /// Retrieves form component or section usage by querying the CMS_Class and CMS_Form tables.
    /// For components: queries CMS_Class.ClassFormDefinition for matching component identifiers, then correlates matching field GUIDs to CMS_Form.FormBuilderLayout.
    /// For sections: queries CMS_Form.FormBuilderLayout for matching section identifiers, then correlates matching field GUIDs to CMS_Class.ClassFormDefinition.
    /// </summary>
    private async Task<FormComponentUsageDetailDto> GetFormComponentUsage(
        string componentIdentifier,
        bool isSection = false,
        CancellationToken cancellationToken = default)
    {
        var result = new FormComponentUsageDetailDto
        {
            ComponentIdentifier = componentIdentifier,
            ComponentType = isSection ? "Section" : "Component",
            FormClasses = [],
            FormBuilderForms = []
        };

        try
        {
            string classQuery;
            string formQuery;
            QueryDataParameters queryDataParameters;

            if (isSection)
            {
                classQuery = @"
                    ;WITH SectionFieldGuids AS (
                        SELECT DISTINCT
                            JSON_VALUE(fc.value, '$.identifier') AS FieldGuid
                        FROM CMS_Form F
                        CROSS APPLY OPENJSON(F.FormBuilderLayout, '$.editableAreas') ea
                        CROSS APPLY OPENJSON(ea.value, '$.sections') s
                        CROSS APPLY OPENJSON(s.value, '$.zones') z
                        CROSS APPLY OPENJSON(z.value, '$.formComponents') fc
                        WHERE JSON_VALUE(s.value, '$.type') = @sectionIdentifier
                    ),
                    ClassFields AS (
                        SELECT
                            C.ClassID,
                            C.ClassDisplayName,
                            C.ClassName,
                            C.ClassXmlSchema,
                            C.ClassFormDefinition,
                            C.ClassTableName,
                            C.ClassLastModified,
                            ClassField.value('@guid', 'nvarchar(36)') AS FieldGuid
                        FROM CMS_Class C
                        CROSS APPLY (SELECT TRY_CAST(C.ClassFormDefinition AS xml) AS ClassFormDefinitionXml) AS ParsedClass
                        CROSS APPLY ParsedClass.ClassFormDefinitionXml.nodes('/form/field') AS X(ClassField)
                        WHERE C.ClassType = 'Form'
                    )
                    SELECT DISTINCT
                        CF.ClassID,
                        CF.ClassDisplayName,
                        CF.ClassName,
                        CF.ClassXmlSchema,
                        CF.ClassFormDefinition,
                        CF.ClassTableName,
                        CF.ClassLastModified
                    FROM ClassFields CF
                    INNER JOIN SectionFieldGuids SFG ON SFG.FieldGuid = CF.FieldGuid
                    ORDER BY CF.ClassDisplayName";

                formQuery = @"
                    ;WITH SectionForms AS (
                        SELECT DISTINCT
                            F.FormID,
                            F.FormName,
                            F.FormDisplayName,
                            F.FormBuilderLayout,
                            F.FormLastModified
                        FROM CMS_Form F
                        CROSS APPLY OPENJSON(F.FormBuilderLayout, '$.editableAreas') ea
                        CROSS APPLY OPENJSON(ea.value, '$.sections') s
                        WHERE JSON_VALUE(s.value, '$.type') = @sectionIdentifier
                    )
                    SELECT
                        SF.FormID,
                        SF.FormName,
                        SF.FormDisplayName,
                        SF.FormBuilderLayout,
                        SF.FormLastModified
                    FROM SectionForms SF
                    ORDER BY SF.FormDisplayName";

                queryDataParameters = new QueryDataParameters
                {
                    { "@sectionIdentifier", componentIdentifier }
                };
            }
            else
            {
                classQuery = @"
                    ;WITH ComponentClassFields AS (
                        SELECT
                            C.ClassID,
                            C.ClassDisplayName,
                            C.ClassName,
                            C.ClassXmlSchema,
                            C.ClassFormDefinition,
                            C.ClassTableName,
                            C.ClassLastModified,
                            ClassField.value('@guid', 'nvarchar(36)') AS FieldGuid
                        FROM CMS_Class C
                        CROSS APPLY (SELECT TRY_CAST(C.ClassFormDefinition AS xml) AS ClassFormDefinitionXml) AS ParsedClass
                        CROSS APPLY ParsedClass.ClassFormDefinitionXml.nodes('/form/field') AS X(ClassField)
                        WHERE C.ClassType = 'Form'
                            AND ClassField.value('(settings/componentidentifier/text())[1]', 'nvarchar(200)') = @componentIdentifier
                    )
                    SELECT DISTINCT
                        CCF.ClassID,
                        CCF.ClassDisplayName,
                        CCF.ClassName,
                        CCF.ClassXmlSchema,
                        CCF.ClassFormDefinition,
                        CCF.ClassTableName,
                        CCF.ClassLastModified
                    FROM ComponentClassFields CCF
                    ORDER BY CCF.ClassDisplayName";

                formQuery = @"
                    ;WITH ComponentClassFields AS (
                        SELECT
                            ClassField.value('@guid', 'nvarchar(36)') AS FieldGuid
                        FROM CMS_Class C
                        CROSS APPLY (SELECT TRY_CAST(C.ClassFormDefinition AS xml) AS ClassFormDefinitionXml) AS ParsedClass
                        CROSS APPLY ParsedClass.ClassFormDefinitionXml.nodes('/form/field') AS X(ClassField)
                        WHERE C.ClassType = 'Form'
                            AND ClassField.value('(settings/componentidentifier/text())[1]', 'nvarchar(200)') = @componentIdentifier
                    ),
                    FormFields AS (
                        SELECT
                            F.FormID,
                            F.FormName,
                            F.FormDisplayName,
                            F.FormBuilderLayout,
                            F.FormLastModified,
                            JSON_VALUE(fc.value, '$.identifier') AS FieldGuid
                        FROM CMS_Form F
                        CROSS APPLY OPENJSON(F.FormBuilderLayout, '$.editableAreas') ea
                        CROSS APPLY OPENJSON(ea.value, '$.sections') s
                        CROSS APPLY OPENJSON(s.value, '$.zones') z
                        CROSS APPLY OPENJSON(z.value, '$.formComponents') fc
                    )
                    SELECT DISTINCT
                        FF.FormID,
                        FF.FormName,
                        FF.FormDisplayName,
                        FF.FormBuilderLayout,
                        FF.FormLastModified
                    FROM FormFields FF
                    INNER JOIN ComponentClassFields CCF ON CCF.FieldGuid = FF.FieldGuid
                    ORDER BY FF.FormDisplayName";

                queryDataParameters = new QueryDataParameters
                {
                    { "@componentIdentifier", componentIdentifier }
                };
            }

            var classParameters = new QueryParameters(classQuery, queryDataParameters, QueryTypeEnum.SQLQuery);
            var formParameters = new QueryParameters(formQuery, queryDataParameters, QueryTypeEnum.SQLQuery);

            DateTime? latestClassModified = null;
            DateTime? latestFormModified = null;

            using (var classConnection = ConnectionHelper.GetConnection())
            {
                var classReader = await classConnection.ExecuteReaderAsync(classParameters, CommandBehavior.Default, cancellationToken);
                if (classReader is not null)
                {
                    var formClasses = new List<FormClassUsageDto>();

                    while (await classReader.ReadAsync(cancellationToken))
                    {
                        int classId = classReader.GetInt32(classReader.GetOrdinal("ClassID"));
                        string classDisplayName = classReader.GetString(classReader.GetOrdinal("ClassDisplayName"));
                        string className = classReader.GetString(classReader.GetOrdinal("ClassName"));
                        string classXmlSchema = classReader.GetString(classReader.GetOrdinal("ClassXmlSchema"));
                        string classFormDefinition = classReader.GetString(classReader.GetOrdinal("ClassFormDefinition"));
                        string classTableName = classReader.GetString(classReader.GetOrdinal("ClassTableName"));
                        int classLastModifiedOrdinal = classReader.GetOrdinal("ClassLastModified");
                        DateTime? classLastModified = !await classReader.IsDBNullAsync(classLastModifiedOrdinal, cancellationToken)
                            ? classReader.GetDateTime(classLastModifiedOrdinal)
                            : null;

                        if (classLastModified is not null &&
                            (latestClassModified is null || classLastModified > latestClassModified))
                        {
                            latestClassModified = classLastModified;
                        }

                        formClasses.Add(new FormClassUsageDto
                        {
                            ClassId = classId,
                            ClassDisplayName = classDisplayName,
                            ClassName = className,
                            ClassXmlSchema = classXmlSchema,
                            ClassFormDefinition = classFormDefinition,
                            ClassTableName = classTableName
                        });
                    }

                    result.FormClasses = formClasses.OrderBy(fc => fc.ClassDisplayName).ToList();
                }
            }

            using (var formConnection = ConnectionHelper.GetConnection())
            {
                var formReader = await formConnection.ExecuteReaderAsync(formParameters, CommandBehavior.Default, cancellationToken);
                if (formReader is not null)
                {
                    var formBuilderForms = new List<FormBuilderFormUsageDto>();

                    while (await formReader.ReadAsync(cancellationToken))
                    {
                        int formId = formReader.GetInt32(formReader.GetOrdinal("FormID"));
                        string formName = formReader.GetString(formReader.GetOrdinal("FormName"));
                        string formDisplayName = formReader.GetString(formReader.GetOrdinal("FormDisplayName"));
                        int formBuilderLayoutOrdinal = formReader.GetOrdinal("FormBuilderLayout");
                        string formBuilderLayout = !await formReader.IsDBNullAsync(formBuilderLayoutOrdinal, cancellationToken)
                            ? formReader.GetString(formBuilderLayoutOrdinal)
                            : string.Empty;
                        int formLastModifiedOrdinal = formReader.GetOrdinal("FormLastModified");
                        DateTime? formLastModified = !await formReader.IsDBNullAsync(formLastModifiedOrdinal, cancellationToken)
                            ? formReader.GetDateTime(formLastModifiedOrdinal)
                            : null;

                        if (formLastModified is not null &&
                            (latestFormModified is null || formLastModified > latestFormModified))
                        {
                            latestFormModified = formLastModified;
                        }

                        formBuilderForms.Add(new FormBuilderFormUsageDto
                        {
                            FormID = formId,
                            FormName = formName,
                            FormDisplayName = formDisplayName,
                            FormBuilderLayout = formBuilderLayout
                        });
                    }

                    result.FormBuilderForms = formBuilderForms;
                }
            }

            if (latestClassModified is not null || latestFormModified is not null)
            {
                result.LastModified = new[] { latestClassModified, latestFormModified }
                    .Where(d => d.HasValue)
                    .Max();
            }

            result.TotalFormClassesUsing = result.FormClasses.Count;
            result.TotalFormBuilderFormsUsing = result.FormBuilderForms.Count;

            return result;
        }
        catch (Exception ex)
        {
            string searchType = isSection ? "section" : "component";
            logger.LogError(ex, "Error retrieving form {SearchType} usage for {ComponentIdentifier}", searchType, componentIdentifier);
            return result;
        }
    }

    /// <summary>
    /// Escapes special characters for SQL LIKE clause.
    /// </summary>
    private static string EscapeForLike(string value) => value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
}

/// <summary>
/// Represents detailed usage information for a specific component across the system.
/// </summary>
public class ComponentUsageDetailDto
{
    /// <summary>
    /// The unique identifier of the component.
    /// </summary>
    public string ComponentIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The type of component ("PageTemplate" or "Widget").
    /// </summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Total number of pages using this component.
    /// </summary>
    public int TotalPagesUsing { get; set; }

    /// <summary>
    /// Total number of language variants across all pages using this component.
    /// </summary>
    public int TotalVariants { get; set; }

    /// <summary>
    /// The date when this component was last used or modified.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// List of pages using this component with their language variants.
    /// </summary>
    public List<PageUsageDto> Pages { get; set; } = [];
}

/// <summary>
/// Represents a page that uses a specific component.
/// </summary>
public class PageUsageDto
{
    /// <summary>
    /// The ID of the web page item.
    /// </summary>
    public int WebPageItemId { get; set; }

    /// <summary>
    /// The ID of the content item.
    /// </summary>
    public int ContentItemId { get; set; }

    /// <summary>
    /// The name of the page.
    /// </summary>
    public string PageName { get; set; } = string.Empty;

    /// <summary>
    /// The URL path of the page.
    /// </summary>
    public string PagePath { get; set; } = string.Empty;

    /// <summary>
    /// The id of the website channel this page belongs to.
    /// </summary>
    public int WebsiteChannelID { get; set; }

    /// <summary>
    /// The display name of the channel this page belongs to.
    /// </summary>
    public string ChannelDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The date when the page was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date when the page was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// The language variants of this page containing component configuration.
    /// </summary>
    public List<PageVariantDto> Variants { get; set; } = [];
}

/// <summary>
/// Represents a language variant of a page's component configuration.
/// </summary>
public class PageVariantDto
{
    /// <summary>
    /// The ID of the content item common data.
    /// </summary>
    public int ContentItemCommonDataId { get; set; }

    /// <summary>
    /// The display name of the language (e.g., "English (United States)").
    /// </summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>
    /// The date when this variant was last modified.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// The JSON configuration for the component in this language variant.
    /// For page templates: ContentItemCommonDataVisualBuilderTemplateConfiguration
    /// For widgets: ContentItemCommonDataVisualBuilderWidgets
    /// </summary>
    public string ConfigurationJson { get; set; } = string.Empty;

    /// <summary>
    /// The type of configuration ("PageTemplate" or "Widget").
    /// </summary>
    public string ConfigurationType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this language variant is published.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// The administration UI path for this page variant.
    /// </summary>
    public string? AdminPath { get; set; }
}

/// <summary>
/// Represents a language variant of a page's component configuration.
/// </summary>
/// <summary>
/// Represents detailed usage information for a specific email builder component across the system.
/// </summary>
public class EmailConfigurationUsageDetailDto
{
    /// <summary>
    /// The unique identifier of the component.
    /// </summary>
    public string ComponentIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The type of component ("EmailWidget" or "EmailTemplate").
    /// </summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Total number of email configurations using this component.
    /// </summary>
    public int TotalEmailConfigurationsUsing { get; set; }

    /// <summary>
    /// Total number of language variants across all email configurations using this component.
    /// </summary>
    public int TotalVariants { get; set; }

    /// <summary>
    /// The date when this component was last used or modified.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// List of email configurations using this component with their language variants.
    /// </summary>
    public List<EmailConfigurationUsageDto> EmailConfigurations { get; set; } = [];
}

/// <summary>
/// Represents an email configuration that uses a specific component.
/// </summary>
public class EmailConfigurationUsageDto
{
    /// <summary>
    /// The ID of the email configuration.
    /// </summary>
    public int EmailConfigurationId { get; set; }

    /// <summary>
    /// The ID of the content item.
    /// </summary>
    public int ContentItemId { get; set; }

    /// <summary>
    /// The name of the email configuration.
    /// </summary>
    public string ConfigurationName { get; set; } = string.Empty;

    /// <summary>
    /// The purpose of the email configuration.
    /// </summary>
    public string ConfigurationPurpose { get; set; } = string.Empty;

    /// <summary>
    /// The id of the email channel this email configuration belongs to.
    /// </summary>
    public int EmailChannelID { get; set; }

    /// <summary>
    /// The display name of the channel this email configuration belongs to.
    /// </summary>
    public string ChannelDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The date when the email configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date when the email configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// The language variants of this email configuration containing component configuration.
    /// </summary>
    public List<EmailConfigurationVariantDto> Variants { get; set; } = [];
}

/// <summary>
/// Represents a language variant of an email configuration's component configuration.
/// </summary>
public class EmailConfigurationVariantDto
{
    /// <summary>
    /// The ID of the content item common data.
    /// </summary>
    public int ContentItemCommonDataId { get; set; }

    /// <summary>
    /// The display name of the language (e.g., "English (United States)").
    /// </summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>
    /// The date when this variant was last modified.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// The JSON configuration for the component in this language variant.
    /// For email templates: ContentItemCommonDataVisualBuilderTemplateConfiguration
    /// For email widgets: ContentItemCommonDataVisualBuilderWidgets
    /// </summary>
    public string ConfigurationJson { get; set; } = string.Empty;

    /// <summary>
    /// The type of configuration ("EmailWidget" or "EmailTemplate").
    /// </summary>
    public string ConfigurationType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this language variant is published.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// The administration UI path for this email configuration variant.
    /// </summary>
    public string? AdminPath { get; set; }
}

/// <summary>
/// Represents detailed usage information for a specific form builder component or section across the system.
/// </summary>
public class FormComponentUsageDetailDto
{
    /// <summary>
    /// The unique identifier of the component or section.
    /// </summary>
    public string ComponentIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The type of component: "Component" or "Section".
    /// </summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Total number of form classes using this component/section (legacy XML-based forms).
    /// </summary>
    public int TotalFormClassesUsing { get; set; }

    /// <summary>
    /// Total number of form builder forms using this component/section (modern JSON-based forms).
    /// </summary>
    public int TotalFormBuilderFormsUsing { get; set; }

    /// <summary>
    /// The date when this component/section was last used or modified.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// List of form classes using this component/section (legacy forms).
    /// </summary>
    public List<FormClassUsageDto> FormClasses { get; set; } = [];

    /// <summary>
    /// List of form builder forms using this component/section (form builder forms).
    /// </summary>
    public List<FormBuilderFormUsageDto> FormBuilderForms { get; set; } = [];
}

/// <summary>
/// Represents a form class that uses a specific component.
/// </summary>
public class FormClassUsageDto
{
    /// <summary>
    /// The ID of the form class.
    /// </summary>
    public int ClassId { get; set; }

    /// <summary>
    /// The display name of the form class.
    /// </summary>
    public string ClassDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The code name of the form class.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// The XML schema definition for the form class.
    /// </summary>
    public string ClassXmlSchema { get; set; } = string.Empty;

    /// <summary>
    /// The form definition XML containing the component configuration.
    /// </summary>
    public string ClassFormDefinition { get; set; } = string.Empty;

    /// <summary>
    /// The table name associated with the form class.
    /// </summary>
    public string ClassTableName { get; set; } = string.Empty;
}
/// <summary>
/// Represents a form builder form that uses a specific component or section.
/// </summary>
public class FormBuilderFormUsageDto
{
    /// <summary>
    /// The ID of the form.
    /// </summary>
    public int FormID { get; set; }

    /// <summary>
    /// The code name of the form.
    /// </summary>
    public string FormName { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the form.
    /// </summary>
    public string FormDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The complete JSON form builder layout configuration.
    /// </summary>
    public string FormBuilderLayout { get; set; } = string.Empty;

    /// <summary>
    /// The administration UI path for this form.
    /// </summary>
    public string? AdminPath { get; set; }
}
